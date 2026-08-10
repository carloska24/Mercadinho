using CaixaMercado.Api.Infrastructure;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaixaMercado.Api.Features.Produtos;

internal static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/produtos").WithTags("Produtos");

        group.MapGet(string.Empty, PesquisarAsync)
            .WithName("PesquisarProdutos")
            .Produces<IReadOnlyList<ProdutoDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/identificadores/{codigo}", ResolverAsync)
            .WithName("ResolverProdutoPorIdentificador")
            .Produces<ProdutoDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> PesquisarAsync(
        string? termo,
        int? limite,
        [FromServices] ICatalogoApplicationService catalogo,
        CancellationToken cancellationToken)
    {
        var limiteEfetivo = limite ?? 50;
        if (limiteEfetivo is < 1 or > 200)
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "LimiteInvalido", "O limite deve estar entre 1 e 200.");

        var resultado = await catalogo.PesquisarAsync(
            new PesquisarProdutosQuery(termo, limiteEfetivo), cancellationToken);
        return resultado.ParaHttp(Results.Ok);
    }

    private static async Task<IResult> ResolverAsync(
        string codigo,
        [FromServices] ICatalogoApplicationService catalogo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return ResultadoOperacaoHttpExtensions.RequisicaoInvalida(
                "IdentificadorProdutoObrigatorio", "O identificador do produto é obrigatório.");

        var resultado = await catalogo.ResolverAsync(
            new ResolverProdutoQuery(codigo), cancellationToken);
        return resultado.ParaHttp(Results.Ok);
    }
}
