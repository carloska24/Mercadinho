namespace CaixaMercado.PDV.Integration.Api;

public sealed class PdvApiOptions
{
    public PdvApiOptions(Uri apiBaseUrl, Guid filialId, Guid terminalId, Guid sessaoCaixaId, Guid operadorId)
    {
        ArgumentNullException.ThrowIfNull(apiBaseUrl);
        if (!apiBaseUrl.IsAbsoluteUri ||
            (!string.Equals(apiBaseUrl.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(apiBaseUrl.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("O endereço da API deve ser absoluto e usar HTTP ou HTTPS.", nameof(apiBaseUrl));
        ValidarId(filialId, nameof(filialId));
        ValidarId(terminalId, nameof(terminalId));
        ValidarId(sessaoCaixaId, nameof(sessaoCaixaId));
        ValidarId(operadorId, nameof(operadorId));
        ApiBaseUrl = apiBaseUrl;
        FilialId = filialId;
        TerminalId = terminalId;
        SessaoCaixaId = sessaoCaixaId;
        OperadorId = operadorId;
    }

    public Uri ApiBaseUrl { get; }
    public Guid FilialId { get; }
    public Guid TerminalId { get; }
    public Guid SessaoCaixaId { get; }
    public Guid OperadorId { get; }

    private static void ValidarId(Guid id, string parametro)
    {
        if (id == Guid.Empty) throw new ArgumentException("O identificador é obrigatório.", parametro);
    }
}
