using CaixaMercado.Application.Operacional.Portas;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class IdempotencyStore(MercadinhoDbContext dbContext) : IIdempotencyStore
{
    public async Task<RegistroIdempotencia?> ObterAsync(
        Guid terminalId,
        string chave,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.RegistrosIdempotencia
            .AsNoTracking()
            .SingleOrDefaultAsync(
                registro => registro.TerminalId == terminalId && registro.Chave == chave,
                cancellationToken);

        return entity?.ParaRegistro();
    }

    public async Task AdicionarAsync(
        RegistroIdempotencia registro,
        CancellationToken cancellationToken)
    {
        await dbContext.RegistrosIdempotencia.AddAsync(
            RegistroIdempotenciaEntity.Criar(registro),
            cancellationToken);
    }
}
