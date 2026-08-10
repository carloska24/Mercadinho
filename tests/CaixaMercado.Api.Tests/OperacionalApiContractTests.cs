using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CaixaMercado.Api.Features.Vendas;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CaixaMercado.Api.Tests;

public sealed class OperacionalApiContractTests : IDisposable
{
    private readonly CatalogoApplicationServiceFake _catalogo = new();
    private readonly VendaApplicationServiceFake _vendas = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OperacionalApiContractTests()
    {
        _factory = new ApiWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICatalogoApplicationService>();
                services.RemoveAll<IVendaApplicationService>();
                services.AddSingleton<ICatalogoApplicationService>(_catalogo);
                services.AddSingleton<IVendaApplicationService>(_vendas);
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task PesquisarProdutos_SemLimite_UsaPadraoERetornaContrato()
    {
        var produto = ProdutoExemplo();
        _catalogo.PesquisarResultado = ResultadoOperacao<IReadOnlyList<ProdutoDto>>.Ok([produto]);

        using var response = await _client.GetAsync("/api/v1/produtos?termo=arroz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProdutoDto[]>();
        Assert.Equal(produto, Assert.Single(body!));
        Assert.Equal(new PesquisarProdutosQuery("arroz", 50), _catalogo.UltimaPesquisa);
    }

    [Fact]
    public async Task ResolverProduto_QuandoNaoExiste_RetornaProblemDetails404()
    {
        _catalogo.ResolverResultado = ResultadoOperacao<ProdutoDto>.Falha(
            CodigoOperacao.ProdutoNaoEncontrado, "Produto não encontrado.");

        using var response = await _client.GetAsync("/api/v1/produtos/identificadores/789123");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("ProdutoNaoEncontrado", body!.RootElement.GetProperty("codigo").GetString());
        Assert.True(body.RootElement.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task CriarVenda_SemIdempotencyKey_Retorna400SemInvocarAplicacao()
    {
        using var response = await _client.PostAsJsonAsync("/api/v1/vendas", CriarVendaRequestExemplo());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(_vendas.UltimaCriacao);
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("ChaveIdempotenciaObrigatoria", body!.RootElement.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task CriarVenda_Valida_Retorna201LocationEEncaminhaChave()
    {
        var request = CriarVendaRequestExemplo();
        var venda = VendaExemplo(request.VendaId, request.TerminalId);
        _vendas.CriarResultado = ResultadoOperacao<VendaDto>.Ok(venda);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/vendas")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("Idempotency-Key", "pdv01-criar-1");

        using var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/vendas/{request.VendaId}", response.Headers.Location?.OriginalString);
        Assert.Equal("pdv01-criar-1", _vendas.UltimaCriacao?.ChaveIdempotencia);
    }

    [Fact]
    public async Task ObterVenda_QuandoNaoExiste_Retorna404()
    {
        _vendas.ObterResultado = ResultadoOperacao<VendaDto>.Falha(
            CodigoOperacao.VendaNaoEncontrada, "Venda não encontrada.");

        using var response = await _client.GetAsync($"/api/v1/vendas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdicionarItem_Valido_EncaminhaVersaoQuantidadeEIdempotencia()
    {
        var vendaId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        _vendas.AdicionarResultado = ResultadoOperacao<VendaDto>.Ok(VendaExemplo(vendaId, terminalId));
        var request = new AdicionarItemVendaRequest(terminalId, "789123", 1.250m, 7);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/vendas/{vendaId}/itens")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("Idempotency-Key", "pdv01-item-1");

        using var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(vendaId, _vendas.UltimaAdicao?.VendaId);
        Assert.Equal(1.250m, _vendas.UltimaAdicao?.Quantidade);
        Assert.Equal(7, _vendas.UltimaAdicao?.VersaoEsperada);
        Assert.Equal("pdv01-item-1", _vendas.UltimaAdicao?.ChaveIdempotencia);
    }

    [Fact]
    public async Task AdicionarItem_ComConflitoDeVersao_Retorna409ProblemDetails()
    {
        var vendaId = Guid.NewGuid();
        _vendas.AdicionarResultado = ResultadoOperacao<VendaDto>.Falha(
            CodigoOperacao.ConflitoVersao, "A venda foi alterada.");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/vendas/{vendaId}/itens")
        {
            Content = JsonContent.Create(new AdicionarItemVendaRequest(Guid.NewGuid(), "001", 1m, 0))
        };
        request.Headers.Add("Idempotency-Key", "pdv01-item-conflito");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("ConflitoVersao", body!.RootElement.GetProperty("codigo").GetString());
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static CriarVendaRequest CriarVendaRequestExemplo() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private static ProdutoDto ProdutoExemplo() => new(
        Guid.NewGuid(), "001", "789123", null, "ARROZ 5KG", UnidadeMedida.Unidade, 24.90m, false);

    private static VendaDto VendaExemplo(Guid vendaId, Guid terminalId) => new(
        vendaId, null, Guid.NewGuid(), terminalId, Guid.NewGuid(), Guid.NewGuid(),
        DateTimeOffset.UtcNow, StatusVendaOperacional.Aberta, 0, 0, 0, 0, 0, []);

    private sealed class CatalogoApplicationServiceFake : ICatalogoApplicationService
    {
        public PesquisarProdutosQuery? UltimaPesquisa { get; private set; }
        public ResultadoOperacao<IReadOnlyList<ProdutoDto>> PesquisarResultado { get; set; } =
            ResultadoOperacao<IReadOnlyList<ProdutoDto>>.Ok([]);
        public ResultadoOperacao<ProdutoDto> ResolverResultado { get; set; } =
            ResultadoOperacao<ProdutoDto>.Falha(CodigoOperacao.ProdutoNaoEncontrado, "Produto não encontrado.");

        public Task<ResultadoOperacao<IReadOnlyList<ProdutoDto>>> PesquisarAsync(
            PesquisarProdutosQuery query, CancellationToken cancellationToken = default)
        {
            UltimaPesquisa = query;
            return Task.FromResult(PesquisarResultado);
        }

        public Task<ResultadoOperacao<ProdutoDto>> ResolverAsync(
            ResolverProdutoQuery query, CancellationToken cancellationToken = default) => Task.FromResult(ResolverResultado);
    }

    private sealed class VendaApplicationServiceFake : IVendaApplicationService
    {
        public CriarVendaCommand? UltimaCriacao { get; private set; }
        public AdicionarItemVendaCommand? UltimaAdicao { get; private set; }
        public ResultadoOperacao<VendaDto> CriarResultado { get; set; } =
            ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Resultado não configurado.");
        public ResultadoOperacao<VendaDto> ObterResultado { get; set; } =
            ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.VendaNaoEncontrada, "Venda não encontrada.");
        public ResultadoOperacao<VendaDto> AdicionarResultado { get; set; } =
            ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Resultado não configurado.");

        public Task<ResultadoOperacao<VendaDto>> CriarAsync(
            CriarVendaCommand command, CancellationToken cancellationToken = default)
        {
            UltimaCriacao = command;
            return Task.FromResult(CriarResultado);
        }

        public Task<ResultadoOperacao<VendaDto>> ObterAsync(
            Guid vendaId, CancellationToken cancellationToken = default) => Task.FromResult(ObterResultado);

        public Task<ResultadoOperacao<VendaDto>> AdicionarItemAsync(
            AdicionarItemVendaCommand command, CancellationToken cancellationToken = default)
        {
            UltimaAdicao = command;
            return Task.FromResult(AdicionarResultado);
        }
    }
}
