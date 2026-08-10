using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class ProdutoRepository(MercadinhoDbContext dbContext) : IProdutoRepository
{
    public async Task<IReadOnlyList<Produto>> PesquisarAsync(
        string? termo,
        int limite,
        CancellationToken cancellationToken)
    {
        var consulta = dbContext.Produtos
            .AsNoTracking()
            .Where(produto => produto.Ativo);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var padrao = $"%{EscaparLike(termo.Trim())}%";
            consulta = consulta.Where(produto =>
                EF.Functions.ILike(produto.CodigoInterno, padrao, "\\") ||
                (produto.Ean != null && EF.Functions.ILike(produto.Ean, padrao, "\\")) ||
                (produto.Plu != null && EF.Functions.ILike(produto.Plu, padrao, "\\")) ||
                EF.Functions.ILike(produto.Descricao, padrao, "\\"));
        }

        return await consulta
            .OrderBy(produto => produto.Descricao)
            .ThenBy(produto => produto.CodigoInterno)
            .Take(limite)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ResultadoBuscaProduto> ResolverPorIdentificadorAsync(
        string identificador,
        CancellationToken cancellationToken)
    {
        var normalizado = identificador.Trim().ToUpperInvariant();
        var encontrados = await dbContext.Produtos
            .AsNoTracking()
            .Where(produto => produto.Ativo &&
                (produto.CodigoInterno == normalizado ||
                 produto.Ean == normalizado ||
                 produto.Plu == normalizado))
            .Take(2)
            .ToArrayAsync(cancellationToken);

        return encontrados.Length switch
        {
            0 => ResultadoBuscaProduto.NaoEncontrado(),
            1 => ResultadoBuscaProduto.Encontrado(encontrados[0]),
            _ => ResultadoBuscaProduto.Ambiguo()
        };
    }

    private static string EscaparLike(string valor) =>
        valor.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
