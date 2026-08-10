using CaixaMercado.Application.Operacional.Portas;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CaixaMercado.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(MercadinhoDbContext dbContext) : IUnitOfWork
{
    private const string ConstraintIdempotencia = "ux_requisicoes_idempotentes_terminal_chave";
    private const string ConstraintSessaoAberta = "ux_sessoes_caixa_terminal_aberta";

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
                ConstraintName: ConstraintSessaoAberta or "pk_sessoes_caixa"
            })
        {
            throw new ConflitoSessaoCaixaException(innerException: exception);
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
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ux_estoque_movimentos_item_tipo" or "pk_estoque_movimentos"
            })
        {
            throw new ConflitoConcorrenciaException(innerException: exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.CheckViolation,
                ConstraintName: "ck_estoque_saldos_quantidade"
            })
        {
            throw new ConflitoConcorrenciaException("O saldo de estoque foi alterado por outra operação.", exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "pk_venda_pagamentos" or "ux_caixa_movimentos_pagamento" or
                    "pk_caixa_movimentos" or "pk_eventos_auditoria"
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
