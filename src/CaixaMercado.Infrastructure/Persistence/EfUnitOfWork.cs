using CaixaMercado.Application.Operacional.Portas;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CaixaMercado.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(MercadinhoDbContext dbContext) : IUnitOfWork
{
    private const string ConstraintIdempotencia = "ux_requisicoes_idempotentes_terminal_chave";

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflitoConcorrenciaException(innerException: exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: ConstraintIdempotencia
            })
        {
            throw new ConflitoIdempotenciaException(innerException: exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ux_venda_itens_venda_sequencial" or "pk_venda_itens"
            })
        {
            throw new ConflitoConcorrenciaException(innerException: exception);
        }
    }

    public Task DescartarAlteracoesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        dbContext.ChangeTracker.Clear();
        return Task.CompletedTask;
    }
}
