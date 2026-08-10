using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class VendaRepository(MercadinhoDbContext dbContext) : IVendaRepository
{
    public Task<Venda?> ObterAsync(Guid vendaId, CancellationToken cancellationToken) =>
        dbContext.Vendas
            .Include(venda => venda.Itens)
            .SingleOrDefaultAsync(venda => venda.Id == vendaId, cancellationToken);

    public async Task AdicionarAsync(Venda venda, CancellationToken cancellationToken)
    {
        await dbContext.Vendas.AddAsync(venda, cancellationToken);
    }

    public Task AtualizarAsync(Venda venda, long versaoEsperada, CancellationToken cancellationToken)
    {
        var entry = dbContext.Entry(venda);
        if (entry.State == EntityState.Detached)
            dbContext.Vendas.Attach(venda);

        entry.Property(item => item.Versao).OriginalValue = versaoEsperada;
        entry.Property(item => item.Versao).IsModified = true;
        return Task.CompletedTask;
    }
}
