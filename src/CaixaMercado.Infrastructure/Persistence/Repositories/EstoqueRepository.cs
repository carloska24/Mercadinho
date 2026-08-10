using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Estoque;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class EstoqueRepository(MercadinhoDbContext dbContext) : IEstoqueRepository
{
    public async Task<bool> TentarBaixarAsync(
        MovimentoEstoque movimento,
        CancellationToken cancellationToken)
    {
        var saldo = await dbContext.SaldosEstoque.SingleOrDefaultAsync(
            item => item.ProdutoId == movimento.ProdutoId,
            cancellationToken);

        if (saldo is null || !saldo.TentarBaixar(movimento.Quantidade))
            return false;

        await dbContext.MovimentosEstoque.AddAsync(movimento, cancellationToken);
        return true;
    }
}
