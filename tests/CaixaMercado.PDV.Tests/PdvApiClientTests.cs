using System.Net;
using System.Net.Http;
using System.Text;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.PDV.Integration.Api;

namespace CaixaMercado.PDV.Tests;

public sealed class PdvApiClientTests
{
    [Fact]
    public async Task PesquisarProdutos_CodificaTermoEDesserializaContrato()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.OK,
            """
            [{"id":"11111111-1111-1111-1111-111111111111","codigoInterno":"001","ean":"789","plu":null,"descricao":"CAFÉ & LEITE","unidadeMedida":1,"precoVenda":12.5,"produtoPesavel":false}]
            """));
        var client = CriarCliente(handler);

        var produtos = await client.PesquisarProdutosAsync("café & leite", 25);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/api/v1/produtos?termo=caf%C3%A9%20%26%20leite&limite=25", request.RequestUri!.PathAndQuery);
        var produto = Assert.Single(produtos);
        Assert.Equal("CAFÉ & LEITE", produto.Descricao);
        Assert.Equal(UnidadeMedida.Unidade, produto.UnidadeMedida);
    }

    [Fact]
    public async Task CriarVenda_EnviaIdempotenciaEIdentidadeDaEstacao()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.Created, VendaJson));
        var options = OpcoesValidas();
        var client = CriarCliente(handler, options);
        var vendaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var venda = await client.CriarVendaAsync(vendaId, "criar-venda-001", "corr-001");

        var request = Assert.Single(handler.Requests);
        Assert.Equal("criar-venda-001", Assert.Single(request.Headers["Idempotency-Key"]));
        Assert.Equal("corr-001", Assert.Single(request.Headers["X-Correlation-ID"]));
        Assert.Contains(options.TerminalId.ToString(), request.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(options.SessaoCaixaId.ToString(), request.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(vendaId, venda.Id);
    }

    [Fact]
    public async Task ErroProblemDetails_ProduzExcecaoOperacionalSemVazarRespostaBruta()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.Conflict,
            """
            {"type":"urn:caixa-mercado:erro:conflito-versao","title":"Conflito","status":409,"detail":"A venda foi alterada.","codigo":"ConflitoVersao","correlationId":"corr-api"}
            """));
        var client = CriarCliente(handler);

        var exception = await Assert.ThrowsAsync<ApiProblemException>(() =>
            client.ObterVendaAsync(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("ConflitoVersao", exception.Codigo);
        Assert.Equal("corr-api", exception.CorrelationId);
        Assert.Equal("A venda foi alterada.", exception.Message);
    }

    [Fact]
    public void Opcoes_RejeitamEnderecoInseguroOuIdentidadeIncompleta()
    {
        var validas = OpcoesValidas();
        Assert.Throws<ArgumentException>(() => new PdvApiOptions(new Uri("ftp://servidor/"),
            validas.FilialId, validas.TerminalId, validas.SessaoCaixaId, validas.OperadorId));
        Assert.Throws<ArgumentException>(() => new PdvApiOptions(validas.ApiBaseUrl,
            validas.FilialId, Guid.Empty, validas.SessaoCaixaId, validas.OperadorId));
    }

    private static CaixaMercadoApiClient CriarCliente(RecordingHandler handler, PdvApiOptions? options = null) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://servidor.local:5080") }, options ?? OpcoesValidas());

    private static PdvApiOptions OpcoesValidas() => new(
        new Uri("http://servidor.local:5080"),
        Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Guid.Parse("20000000-0000-0000-0000-000000000001"),
        Guid.Parse("30000000-0000-0000-0000-000000000001"),
        Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static HttpResponseMessage Json(HttpStatusCode status, string content) => new(status)
    {
        Content = new StringContent(content, Encoding.UTF8, "application/json")
    };

    private const string VendaJson =
        """
        {"id":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","numero":null,"filialId":"10000000-0000-0000-0000-000000000001","terminalId":"20000000-0000-0000-0000-000000000001","sessaoCaixaId":"30000000-0000-0000-0000-000000000001","operadorId":"40000000-0000-0000-0000-000000000001","criadaEmUtc":"2026-08-10T12:00:00+00:00","status":1,"versao":0,"quantidadeTotal":0,"subtotal":0,"desconto":0,"total":0,"itens":[]}
        """;

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(
                header => header.Key,
                header => header.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
            Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, headers,
                request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult()));
            return Task.FromResult(responder(request));
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        Uri RequestUri,
        IReadOnlyDictionary<string, string[]> Headers,
        string? Body);
}
