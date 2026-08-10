using CaixaMercado.Api.Infrastructure;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaixaMercado.Api.Features.Vendas;

internal static class VendasEndpoints
{
    private const string IdempotencyHeader = "Idempotency-Key";

    public static IEndpointRouteBuilder MapVendasEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/vendas").WithTags("Vendas");

        group.MapPost(string.Empty, CriarAsync)
            .WithName("CriarVenda")
            .Produces<VendaDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", ObterAsync)
            .WithName("ObterVenda")
            .Produces<VendaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/itens", AdicionarItemAsync)
            .WithName("AdicionarItemVenda")
            .Produces<VendaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<IResult> CriarAsync(
        CriarVendaRequest request,
        HttpRequest httpRequest,
        [FromServices] IVendaApplicationService vendas,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdempotencyKey(httpRequest, out var chave, out var erro)) return erro!;
        if (request.VendaId == Guid.Empty || request.FilialId == Guid.Empty || request.TerminalId == Guid.Empty ||
            request.SessaoCaixaId == Guid.Empty || request.OperadorId == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresObrigatorios", "Todos os identificadores são obrigatórios.");

        var resultado = await vendas.CriarAsync(new CriarVendaCommand(
            request.VendaId,
            request.FilialId,
            request.TerminalId,
            request.SessaoCaixaId,
            request.OperadorId,
            chave!), cancellationToken);

        return resultado.ParaHttp(venda => Results.Created($"/api/v1/vendas/{venda.Id}", venda));
    }

    private static async Task<IResult> ObterAsync(
        Guid id,
        [FromServices] IVendaApplicationService vendas,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "VendaObrigatoria", "O identificador da venda é obrigatório.");

        var resultado = await vendas.ObterAsync(id, cancellationToken);
        return resultado.ParaHttp(Results.Ok);
    }

    private static async Task<IResult> AdicionarItemAsync(
        Guid id,
        AdicionarItemVendaRequest request,
        HttpRequest httpRequest,
        [FromServices] IVendaApplicationService vendas,
        CancellationToken cancellationToken)
    {
        if (!TryGetIdempotencyKey(httpRequest, out var chave, out var erro)) return erro!;
        if (id == Guid.Empty || request.TerminalId == Guid.Empty)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadoresObrigatorios", "Venda e terminal são obrigatórios.");
        if (string.IsNullOrWhiteSpace(request.IdentificadorProduto))
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadorProdutoObrigatorio", "O identificador do produto é obrigatório.");
        if (request.Quantidade <= 0m)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "QuantidadeInvalida", "A quantidade deve ser maior que zero.");
        if (request.VersaoEsperada < 0)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "VersaoEsperadaInvalida", "A versão esperada é inválida.");

        var resultado = await vendas.AdicionarItemAsync(new AdicionarItemVendaCommand(
            id,
            request.TerminalId,
            request.IdentificadorProduto,
            request.Quantidade,
            request.VersaoEsperada,
            chave!), cancellationToken);

        return resultado.ParaHttp(Results.Ok);
    }

    private static bool TryGetIdempotencyKey(HttpRequest request, out string? chave, out IResult? erro)
    {
        chave = request.Headers[IdempotencyHeader].FirstOrDefault()?.Trim();
        if (!string.IsNullOrWhiteSpace(chave) && chave.Length <= 100)
        {
            erro = null;
            return true;
        }

        erro = ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
            "ChaveIdempotenciaObrigatoria",
            $"O cabeçalho {IdempotencyHeader} é obrigatório e deve ter até 100 caracteres.");
        return false;
    }
}

public sealed record CriarVendaRequest(
    Guid VendaId,
    Guid FilialId,
    Guid TerminalId,
    Guid SessaoCaixaId,
    Guid OperadorId);

public sealed record AdicionarItemVendaRequest(
    Guid TerminalId,
    string IdentificadorProduto,
    decimal Quantidade,
    long VersaoEsperada);
