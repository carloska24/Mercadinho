using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Vendas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class MovimentoCaixaRepository(MercadinhoDbContext dbContext) : IMovimentoCaixaRepository
{
    public Task AdicionarAsync(
        IReadOnlyCollection<MovimentoCaixa> movimentos,
        CancellationToken cancellationToken) =>
        dbContext.MovimentosCaixa.AddRangeAsync(movimentos, cancellationToken);

    public async Task<decimal> ObterRecebimentoLiquidoDinheiroAsync(
        Guid sessaoCaixaId,
        CancellationToken cancellationToken) =>
        await dbContext.MovimentosCaixa
            .AsNoTracking()
            .Where(movimento => movimento.SessaoCaixaId == sessaoCaixaId &&
                movimento.Forma == FormaPagamentoOperacional.Dinheiro)
            .Select(movimento => (decimal?)movimento.ValorLiquido)
            .SumAsync(cancellationToken) ?? 0m;
}
