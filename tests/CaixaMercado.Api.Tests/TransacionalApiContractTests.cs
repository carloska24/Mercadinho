using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CaixaMercado.Api.Features.Caixas;
using CaixaMercado.Api.Features.Vendas;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CaixaMercado.Api.Tests;

public sealed class TransacionalApiContractTests : IDisposable
{
    private readonly SessaoCaixaApplicationServiceFake _sessoes = new();
    private readonly FinalizacaoVendaApplicationServiceFake _finalizacao = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransacionalApiContractTests()
    {
        _factory = new ApiWebApplicationFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISessaoCaixaApplicationService>();
                services.RemoveAll<IFinalizacaoVendaApplicationService>();
                services.AddSingleton<ISessaoCaixaApplicationService>(_sessoes);
                services.AddSingleton<IFinalizacaoVendaApplicationService>(_finalizacao);
            });
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task AbrirSessao_Valida_Retorna201LocationEEncaminhaIdempotencia()
    {
        var request = AbrirSessaoRequestExemplo();
        _sessoes.AbrirResultado = ResultadoOperacao<SessaoCaixaDto>.Ok(SessaoExemplo(request));
        using var httpRequest = CriarPost("/api/v1/caixas/sessoes", request, "caixa-abrir-1");

        using var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/v1/caixas/sessoes/{request.SessaoCaixaId}", response.Headers.Location?.OriginalString);
        Assert.Equal("caixa-abrir-1", _sessoes.UltimaAbertura?.ChaveIdempotencia);
    }

    [Fact]
    public async Task AbrirSessao_QuandoTerminalJaPossuiSessao_Retorna409()
    {
        _sessoes.AbrirResultado = ResultadoOperacao<SessaoCaixaDto>.Falha(
            CodigoOperacao.SessaoCaixaJaAberta, "Já existe uma sessão aberta para o terminal.");
        using var request = CriarPost("/api/v1/caixas/sessoes", AbrirSessaoRequestExemplo(), "caixa-abrir-2");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertCodigoAsync(response, "SessaoCaixaJaAberta");
    }

    [Fact]
    public async Task FecharSessao_Inexistente_Retorna404()
    {
        var sessaoId = Guid.NewGuid();
        _sessoes.FecharResultado = ResultadoOperacao<SessaoCaixaDto>.Falha(
            CodigoOperacao.SessaoCaixaNaoEncontrada, "Sessão de caixa não encontrada.");
        var body = new FecharSessaoCaixaRequest(Guid.NewGuid(), Guid.NewGuid(), 119.50m, 0);
        using var request = CriarPost($"/api/v1/caixas/sessoes/{sessaoId}/fechamento", body, "caixa-fechar-1");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(sessaoId, _sessoes.UltimoFechamento?.SessaoCaixaId);
    }

    [Fact]
    public async Task FinalizarComDinheiro_SemStatusExplicito_MantemAprovacaoNoApplication()
    {
        var vendaId = Guid.NewGuid();
        _finalizacao.Resultado = ResultadoOperacao<FinalizacaoVendaDto>.Ok(
            FinalizacaoExemplo(vendaId, 2.50m));
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.Dinheiro, 25m,
            ValorRecebidoDinheiro: 27.50m));
        using var request = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "venda-finalizar-dinheiro");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pagamento = Assert.Single(_finalizacao.UltimaFinalizacao!.Pagamentos);
        Assert.Equal(StatusPagamentoOperacional.Pendente, pagamento.Status);
        Assert.Equal(27.50m, pagamento.ValorRecebidoDinheiro);
        Assert.False(string.IsNullOrWhiteSpace(_finalizacao.UltimaFinalizacao.CorrelationId));
    }

    [Fact]
    public async Task FinalizarComEletronicoSemIntegracao_NaoInvocaApplicationERetorna422Estavel()
    {
        var vendaId = Guid.NewGuid();
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.Pix, 25m));
        using var request = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "venda-finalizar-pix-pendente");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(_finalizacao.UltimaFinalizacao);
        await AssertCodigoAsync(response, "IntegracaoPagamentoIndisponivel");
    }

    [Fact]
    public async Task FinalizarComEletronicoMarcadoAprovadoSemReferencia_TambemFicaBloqueado()
    {
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.CartaoDebito, 25m,
            StatusPagamentoOperacional.Aprovado));
        using var request = CriarPost($"/api/v1/vendas/{Guid.NewGuid()}/finalizacao", body, "venda-finalizar-debito-invalido");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(_finalizacao.UltimaFinalizacao);
        await AssertCodigoAsync(response, "IntegracaoPagamentoIndisponivel");
    }

    [Fact]
    public async Task FinalizarComEletronicoExplicitamenteAprovado_NaoConfiaNoPayloadDoCliente()
    {
        var vendaId = Guid.NewGuid();
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.Pix, 25m,
            StatusPagamentoOperacional.Aprovado, ReferenciaExterna: "pix-e2e-123"));
        using var request = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "venda-finalizar-pix-aprovado");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(_finalizacao.UltimaFinalizacao);
        await AssertCodigoAsync(response, "IntegracaoPagamentoIndisponivel");
    }

    [Fact]
    public async Task Finalizar_SemEstoque_Retorna409()
    {
        var vendaId = Guid.NewGuid();
        _finalizacao.Resultado = ResultadoOperacao<FinalizacaoVendaDto>.Falha(
            CodigoOperacao.EstoqueInsuficiente, "Estoque insuficiente para finalizar a venda.");
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.Dinheiro, 25m));
        using var request = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "venda-finalizar-sem-estoque");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await AssertCodigoAsync(response, "EstoqueInsuficiente");
    }

    [Fact]
    public async Task Finalizar_SemPagamentos_Retorna400SemApplication()
    {
        var body = new FinalizarVendaRequest(Guid.NewGuid(), Guid.NewGuid(), 0, []);
        using var request = CriarPost($"/api/v1/vendas/{Guid.NewGuid()}/finalizacao", body, "venda-finalizar-vazia");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(_finalizacao.UltimaFinalizacao);
    }

    [Fact]
    public async Task RepetirFinalizacao_MesmaIdempotenciaComCorrelationDiferente_PreservaChaveOperacional()
    {
        var vendaId = Guid.NewGuid();
        _finalizacao.Resultado = ResultadoOperacao<FinalizacaoVendaDto>.Ok(FinalizacaoExemplo(vendaId, 0m));
        var body = FinalizarRequestExemplo(new PagamentoRequest(
            Guid.NewGuid(), FormaPagamentoOperacional.Dinheiro, 25m));
        using var primeira = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "replay-finalizacao-1");
        primeira.Headers.Add("X-Correlation-ID", "correlation-primeira");
        using var segunda = CriarPost($"/api/v1/vendas/{vendaId}/finalizacao", body, "replay-finalizacao-1");
        segunda.Headers.Add("X-Correlation-ID", "correlation-segunda");

        using var resposta1 = await _client.SendAsync(primeira);
        using var resposta2 = await _client.SendAsync(segunda);

        Assert.Equal(HttpStatusCode.OK, resposta1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, resposta2.StatusCode);
        Assert.Collection(_finalizacao.Finalizacoes,
            command =>
            {
                Assert.Equal("replay-finalizacao-1", command.ChaveIdempotencia);
                Assert.Equal("correlation-primeira", command.CorrelationId);
            },
            command =>
            {
                Assert.Equal("replay-finalizacao-1", command.ChaveIdempotencia);
                Assert.Equal("correlation-segunda", command.CorrelationId);
            });
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static HttpRequestMessage CriarPost<T>(string uri, T body, string idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return request;
    }

    private static async Task AssertCodigoAsync(HttpResponseMessage response, string codigo)
    {
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal(codigo, body!.RootElement.GetProperty("codigo").GetString());
    }

    private static AbrirSessaoCaixaRequest AbrirSessaoRequestExemplo() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

    private static SessaoCaixaDto SessaoExemplo(AbrirSessaoCaixaRequest request) => new(
        request.SessaoCaixaId, request.FilialId, request.TerminalId, request.OperadorId,
        request.ValorAbertura, DateTimeOffset.UtcNow, StatusSessaoCaixa.Aberta,
        null, null, null, null, null, 0);

    private static FinalizarVendaRequest FinalizarRequestExemplo(PagamentoRequest pagamento) =>
        new(Guid.NewGuid(), Guid.NewGuid(), 0, [pagamento]);

    private static FinalizacaoVendaDto FinalizacaoExemplo(Guid vendaId, decimal troco) => new(
        vendaId, 1001, 1, StatusVendaOperacional.Finalizada, 25m, troco, 1, 1);

    private sealed class SessaoCaixaApplicationServiceFake : ISessaoCaixaApplicationService
    {
        public AbrirSessaoCaixaCommand? UltimaAbertura { get; private set; }
        public FecharSessaoCaixaCommand? UltimoFechamento { get; private set; }
        public ResultadoOperacao<SessaoCaixaDto> AbrirResultado { get; set; } =
            ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Não configurado.");
        public ResultadoOperacao<SessaoCaixaDto> FecharResultado { get; set; } =
            ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Não configurado.");

        public Task<ResultadoOperacao<SessaoCaixaDto>> AbrirAsync(
            AbrirSessaoCaixaCommand command, CancellationToken cancellationToken = default)
        {
            UltimaAbertura = command;
            return Task.FromResult(AbrirResultado);
        }

        public Task<ResultadoOperacao<SessaoCaixaDto>> FecharAsync(
            FecharSessaoCaixaCommand command, CancellationToken cancellationToken = default)
        {
            UltimoFechamento = command;
            return Task.FromResult(FecharResultado);
        }
    }

    private sealed class FinalizacaoVendaApplicationServiceFake : IFinalizacaoVendaApplicationService
    {
        public FinalizarVendaCommand? UltimaFinalizacao { get; private set; }
        public List<FinalizarVendaCommand> Finalizacoes { get; } = [];
        public ResultadoOperacao<FinalizacaoVendaDto> Resultado { get; set; } =
            ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Não configurado.");

        public Task<ResultadoOperacao<FinalizacaoVendaDto>> FinalizarAsync(
            FinalizarVendaCommand command, CancellationToken cancellationToken = default)
        {
            UltimaFinalizacao = command;
            Finalizacoes.Add(command);
            return Task.FromResult(Resultado);
        }
    }
}
