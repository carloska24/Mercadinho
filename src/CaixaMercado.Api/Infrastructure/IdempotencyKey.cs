namespace CaixaMercado.Api.Infrastructure;

internal static class IdempotencyKey
{
    private const string HeaderName = "Idempotency-Key";

    public static bool TryGet(HttpRequest request, out string? chave, out IResult? erro)
    {
        chave = request.Headers[HeaderName].FirstOrDefault()?.Trim();
        if (!string.IsNullOrWhiteSpace(chave) && chave.Length <= 100)
        {
            erro = null;
            return true;
        }

        erro = ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
            "ChaveIdempotenciaObrigatoria",
            $"O cabeçalho {HeaderName} é obrigatório e deve ter até 100 caracteres.");
        return false;
    }
}
