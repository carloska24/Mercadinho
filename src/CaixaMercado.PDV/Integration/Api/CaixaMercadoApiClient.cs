using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CaixaMercado.Application.Operacional.Contratos;

namespace CaixaMercado.PDV.Integration.Api;

public interface ICaixaMercadoApiClient
{
    Task<IReadOnlyList<ProdutoDto>> PesquisarProdutosAsync(string? termo, int limite = 50,
        CancellationToken cancellationToken = default);
    Task<VendaDto> CriarVendaAsync(Guid vendaId, string chaveIdempotencia, string correlationId,
        CancellationToken cancellationToken = default);
    Task<VendaDto> ObterVendaAsync(Guid vendaId, CancellationToken cancellationToken = default);
}

public sealed class CaixaMercadoApiClient : ICaixaMercadoApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly PdvApiOptions _options;

    public CaixaMercadoApiClient(HttpClient httpClient, PdvApiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient.BaseAddress = options.ApiBaseUrl;
    }

    public async Task<IReadOnlyList<ProdutoDto>> PesquisarProdutosAsync(string? termo, int limite = 50,
        CancellationToken cancellationToken = default)
    {
        if (limite is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(limite));
        var query = string.IsNullOrWhiteSpace(termo)
            ? $"?limite={limite}"
            : $"?termo={Uri.EscapeDataString(termo.Trim())}&limite={limite}";
        using var response = await _httpClient.GetAsync($"/api/v1/produtos{query}", cancellationToken);
        return await LerRespostaAsync<IReadOnlyList<ProdutoDto>>(response, cancellationToken);
    }

    public async Task<VendaDto> CriarVendaAsync(Guid vendaId, string chaveIdempotencia, string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (vendaId == Guid.Empty) throw new ArgumentException("A venda é obrigatória.", nameof(vendaId));
        using var request = CriarRequisicaoMutavel(HttpMethod.Post, "/api/v1/vendas", chaveIdempotencia, correlationId);
        request.Content = JsonContent.Create(new
        {
            VendaId = vendaId,
            _options.FilialId,
            _options.TerminalId,
            _options.SessaoCaixaId,
            _options.OperadorId
        }, options: JsonOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await LerRespostaAsync<VendaDto>(response, cancellationToken);
    }

    public async Task<VendaDto> ObterVendaAsync(Guid vendaId, CancellationToken cancellationToken = default)
    {
        if (vendaId == Guid.Empty) throw new ArgumentException("A venda é obrigatória.", nameof(vendaId));
        using var response = await _httpClient.GetAsync($"/api/v1/vendas/{vendaId:D}", cancellationToken);
        return await LerRespostaAsync<VendaDto>(response, cancellationToken);
    }

    private static HttpRequestMessage CriarRequisicaoMutavel(HttpMethod method, string rota,
        string chaveIdempotencia, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            throw new ArgumentException("A chave de idempotência é obrigatória.", nameof(chaveIdempotencia));
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("A correlação é obrigatória.", nameof(correlationId));
        var request = new HttpRequestMessage(method, rota);
        request.Headers.Add("Idempotency-Key", chaveIdempotencia.Trim());
        request.Headers.Add("X-Correlation-ID", correlationId.Trim());
        return request;
    }

    private static async Task<T> LerRespostaAsync<T>(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var dados = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return dados ?? throw new InvalidOperationException("A API retornou uma resposta vazia.");
        }

        var codigo = "ErroApi";
        var mensagem = $"A API retornou o status {(int)response.StatusCode}.";
        string? correlationId = null;
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;
            if (root.TryGetProperty("codigo", out var codigoJson)) codigo = codigoJson.GetString() ?? codigo;
            if (root.TryGetProperty("detail", out var detalheJson)) mensagem = detalheJson.GetString() ?? mensagem;
            if (root.TryGetProperty("correlationId", out var correlationJson)) correlationId = correlationJson.GetString();
        }
        catch (JsonException)
        {
            // Conteúdo inválido do servidor não é exibido ao operador.
        }
        throw new ApiProblemException(response.StatusCode, codigo, mensagem, correlationId);
    }
}
