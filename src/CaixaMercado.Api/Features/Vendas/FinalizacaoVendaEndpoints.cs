using CaixaMercado.Api.Infrastructure;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.AspNetCore.Mvc;

namespace CaixaMercado.Api.Features.Vendas;

internal static class FinalizacaoVendaEndpoints
{
    public static IEndpointRouteBuilder MapFinalizacaoVendaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/vendas/{id:guid}/finalizacao", FinalizarAsync)
            .WithTags("Vendas")
            .WithName("FinalizarVenda")
            .Produces<FinalizacaoVendaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<IResult> FinalizarAsync(
        Guid id,
        FinalizarVendaRequest request,
        HttpRequest httpRequest,
        HttpContext httpContext,
        [FromServices] IFinalizacaoVendaApplicationService finalizacao,
        CancellationToken cancellationToken)
    {
        if (!IdempotencyKey.TryGet(httpRequest, out var chave, out var erro)) return erro!;
        var erroValidacao = Validar(id, request);
        if (erroValidacao is not null) return erroValidacao;

        var pagamentos = request.Pagamentos.Select(pagamento => new PagamentoCommand(
            pagamento.PagamentoId,
            pagamento.Forma,
            pagamento.ValorAplicado,
            pagamento.Status ?? StatusPagamentoOperacional.Pendente,
            pagamento.ValorRecebidoDinheiro,
            pagamento.ReferenciaExterna)).ToArray();

        var resultado = await finalizacao.FinalizarAsync(new FinalizarVendaCommand(
            id,
            request.TerminalId,
            request.OperadorId,
            request.VersaoEsperada,
            pagamentos,
            chave!,
            request.AutorizacaoId,
            httpContext.TraceIdentifier), cancellationToken);

        return resultado.ParaHttp(Results.Ok);
    }

    private static IResult? Validar(Guid vendaId, FinalizarVendaRequest request)
    {
        if (vendaId == Guid.Empty || request.TerminalId == Guid.Empty || request.OperadorId == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresObrigatorios", "Venda, terminal e operador são obrigatórios.");
        if (request.VersaoEsperada < 0)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "VersaoEsperadaInvalida", "A versão esperada é inválida.");
        if (request.Pagamentos is null || request.Pagamentos.Count is < 1 or > 20)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "PagamentosInvalidos", "Informe entre um e vinte pagamentos.");
        if (request.Pagamentos.Any(p => p.PagamentoId == Guid.Empty) ||
            request.Pagamentos.Select(p => p.PagamentoId).Distinct().Count() != request.Pagamentos.Count)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresPagamentoInvalidos", "Os pagamentos devem possuir identificadores únicos.");

        foreach (var pagamento in request.Pagamentos)
        {
            if (!Enum.IsDefined(pagamento.Forma) ||
                (pagamento.Status.HasValue && !Enum.IsDefined(pagamento.Status.Value)))
                return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                    "PagamentoInvalido", "Forma ou situação de pagamento inválida.");
            if (pagamento.ValorAplicado <= 0m || decimal.Round(pagamento.ValorAplicado, 2) != pagamento.ValorAplicado)
                return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                    "ValorPagamentoInvalido", "O valor aplicado deve ser positivo e ter até duas casas decimais.");
            if (pagamento.ReferenciaExterna?.Length > 100)
                return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                    "ReferenciaPagamentoInvalida", "A referência externa deve ter até 100 caracteres.");

            if (pagamento.Forma == FormaPagamentoOperacional.Dinheiro)
            {
                if (pagamento.ValorRecebidoDinheiro is { } recebido &&
                    (recebido < pagamento.ValorAplicado || decimal.Round(recebido, 2) != recebido))
                    return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                        "ValorRecebidoInvalido", "O valor recebido em dinheiro não pode ser menor que o valor aplicado.");
                continue;
            }

            return ResultadoOperacaoHttpExtensions.IntegracaoPagamentoIndisponivel();
        }

        return null;
    }
}

public sealed record FinalizarVendaRequest(
    Guid TerminalId,
    Guid OperadorId,
    long VersaoEsperada,
    IReadOnlyList<PagamentoRequest> Pagamentos,
    Guid? AutorizacaoId = null);

public sealed record PagamentoRequest(
    Guid PagamentoId,
    FormaPagamentoOperacional Forma,
    decimal ValorAplicado,
    StatusPagamentoOperacional? Status = null,
    decimal? ValorRecebidoDinheiro = null,
    string? ReferenciaExterna = null);
