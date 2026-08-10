using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class PagamentoVendaRepository(MercadinhoDbContext dbContext) : IPagamentoVendaRepository
{
    public Task AdicionarAsync(
        IReadOnlyCollection<PagamentoVenda> pagamentos,
        CancellationToken cancellationToken) =>
        dbContext.PagamentosVenda.AddRangeAsync(pagamentos, cancellationToken);
}
