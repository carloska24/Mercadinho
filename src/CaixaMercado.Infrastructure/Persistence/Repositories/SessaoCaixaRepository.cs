using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Caixas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class SessaoCaixaRepository(MercadinhoDbContext dbContext) : ISessaoCaixaRepository
{
    public Task<SessaoCaixa?> ObterAsync(Guid sessaoId, CancellationToken cancellationToken) =>
        dbContext.SessoesCaixa.SingleOrDefaultAsync(sessao => sessao.Id == sessaoId, cancellationToken);

    public Task<SessaoCaixa?> ObterAbertaPorTerminalAsync(Guid terminalId, CancellationToken cancellationToken) =>
        dbContext.SessoesCaixa.SingleOrDefaultAsync(
            sessao => sessao.TerminalId == terminalId && sessao.Status == StatusSessaoCaixa.Aberta,
            cancellationToken);

    public async Task AdicionarAsync(SessaoCaixa sessao, CancellationToken cancellationToken)
    {
        await dbContext.SessoesCaixa.AddAsync(sessao, cancellationToken);
    }

    public Task AtualizarAsync(SessaoCaixa sessao, long versaoEsperada, CancellationToken cancellationToken)
    {
        var entry = dbContext.Entry(sessao);
        if (entry.State == EntityState.Detached)
            dbContext.SessoesCaixa.Attach(sessao);

        entry.Property(item => item.Versao).OriginalValue = versaoEsperada;
        entry.Property(item => item.Versao).IsModified = true;
        return Task.CompletedTask;
    }
}
