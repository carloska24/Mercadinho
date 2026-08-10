using CaixaMercado.Api.Infrastructure;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaixaMercado.Api.Features.Caixas;

internal static class CaixasEndpoints
{
    public static IEndpointRouteBuilder MapCaixasEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/caixas/sessoes").WithTags("Caixas");

        group.MapPost(string.Empty, AbrirAsync)
            .WithName("AbrirSessaoCaixa")
            .Produces<SessaoCaixaDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/fechamento", FecharAsync)
            .WithName("FecharSessaoCaixa")
            .Produces<SessaoCaixaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<IResult> AbrirAsync(
        AbrirSessaoCaixaRequest request,
        HttpRequest httpRequest,
        [FromServices] ISessaoCaixaApplicationService sessoes,
        CancellationToken cancellationToken)
    {
        if (!IdempotencyKey.TryGet(httpRequest, out var chave, out var erro)) return erro!;
        if (request.SessaoCaixaId == Guid.Empty || request.FilialId == Guid.Empty ||
            request.TerminalId == Guid.Empty || request.OperadorId == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresObrigatorios", "Todos os identificadores são obrigatórios.");
        if (!ValorMonetarioValido(request.ValorAbertura))
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "ValorAberturaInvalido", "O valor de abertura deve ser positivo ou zero e ter até duas casas decimais.");

        var resultado = await sessoes.AbrirAsync(new AbrirSessaoCaixaCommand(
            request.SessaoCaixaId,
            request.FilialId,
            request.TerminalId,
            request.OperadorId,
            request.ValorAbertura,
            chave!), cancellationToken);

        return resultado.ParaHttp(sessao =>
            Results.Created($"/api/v1/caixas/sessoes/{sessao.Id}", sessao));
    }

    private static async Task<IResult> FecharAsync(
        Guid id,
        FecharSessaoCaixaRequest request,
        HttpRequest httpRequest,
        [FromServices] ISessaoCaixaApplicationService sessoes,
        CancellationToken cancellationToken)
    {
        if (!IdempotencyKey.TryGet(httpRequest, out var chave, out var erro)) return erro!;
        if (id == Guid.Empty || request.TerminalId == Guid.Empty || request.OperadorId == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresObrigatorios", "Sessão, terminal e operador são obrigatórios.");
        if (!ValorMonetarioValido(request.ValorContado))
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "ValorFechamentoInvalido", "O valor contado deve ser positivo ou zero e ter até duas casas decimais.");
        if (request.VersaoEsperada < 0)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "VersaoEsperadaInvalida", "A versão esperada é inválida.");

        var resultado = await sessoes.FecharAsync(new FecharSessaoCaixaCommand(
            id,
            request.TerminalId,
            request.OperadorId,
            request.ValorContado,
            request.VersaoEsperada,
            chave!), cancellationToken);

        return resultado.ParaHttp(Results.Ok);
    }

    private static bool ValorMonetarioValido(decimal valor) => valor >= 0m && decimal.Round(valor, 2) == valor;
}

public sealed record AbrirSessaoCaixaRequest(
    Guid SessaoCaixaId,
    Guid FilialId,
    Guid TerminalId,
    Guid OperadorId,
    decimal ValorAbertura);

public sealed record FecharSessaoCaixaRequest(
    Guid TerminalId,
    Guid OperadorId,
    decimal ValorContado,
    long VersaoEsperada);
