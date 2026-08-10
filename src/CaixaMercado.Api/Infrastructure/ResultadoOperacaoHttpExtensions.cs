using CaixaMercado.Application.Operacional.Contratos;

namespace CaixaMercado.Api.Infrastructure;

internal static class ResultadoOperacaoHttpExtensions
{
    public static IResult ParaHttp<T>(this ResultadoOperacao<T> resultado, Func<T, IResult> sucesso)
    {
        if (resultado.Sucesso && resultado.Dados is not null)
            return sucesso(resultado.Dados);

        var status = resultado.Codigo switch
        {
            CodigoOperacao.RequisicaoInvalida => StatusCodes.Status400BadRequest,
            CodigoOperacao.ProdutoNaoEncontrado or
                CodigoOperacao.VendaNaoEncontrada or
                CodigoOperacao.SessaoCaixaNaoEncontrada => StatusCodes.Status404NotFound,
            CodigoOperacao.RegraNegocioViolada => StatusCodes.Status422UnprocessableEntity,
            CodigoOperacao.IdentificadorProdutoAmbiguo or
                CodigoOperacao.ConflitoVersao or
                CodigoOperacao.ChaveIdempotenciaReutilizada or
                CodigoOperacao.ConflitoIdempotencia or
                CodigoOperacao.SessaoCaixaJaAberta or
                CodigoOperacao.SessaoCaixaFechada or
                CodigoOperacao.EstoqueInsuficiente => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var codigo = resultado.Codigo.ToString();
        return Results.Problem(
            statusCode: status,
            title: Titulo(status),
            detail: resultado.Mensagem ?? "Não foi possível concluir a operação.",
            type: $"urn:caixa-mercado:erro:{ParaKebabCase(codigo)}",
            extensions: new Dictionary<string, object?> { ["codigo"] = codigo });
    }

    public static IResult RequisicaoInvalida(string codigo, string detalhe) =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Requisição inválida",
            detail: detalhe,
            type: $"urn:caixa-mercado:erro:{ParaKebabCase(codigo)}",
            extensions: new Dictionary<string, object?> { ["codigo"] = codigo });

    public static IResult IntegracaoPagamentoIndisponivel() =>
        Results.Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Integração de pagamento indisponível",
            detail: "Pix, cartão e voucher somente poderão ser finalizados após confirmação de um provedor confiável.",
            type: "urn:caixa-mercado:erro:integracao-pagamento-indisponivel",
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = "IntegracaoPagamentoIndisponivel"
            });

    private static string Titulo(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida",
        StatusCodes.Status404NotFound => "Recurso não encontrado",
        StatusCodes.Status409Conflict => "Conflito",
        StatusCodes.Status422UnprocessableEntity => "Regra de negócio não atendida",
        _ => "Erro interno"
    };

    private static string ParaKebabCase(string value) =>
        string.Concat(value.Select((character, index) =>
                char.IsUpper(character) && index > 0 ? $"-{char.ToLowerInvariant(character)}" : char.ToLowerInvariant(character).ToString()));
}
