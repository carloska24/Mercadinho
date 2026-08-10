using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Auditoria;

namespace CaixaMercado.Infrastructure.Persistence.Repositories;

internal sealed class AuditoriaRepository(MercadinhoDbContext dbContext) : IAuditoriaRepository
{
    public async Task AdicionarAsync(EventoAuditoria evento, CancellationToken cancellationToken)
    {
        await dbContext.EventosAuditoria.AddAsync(evento, cancellationToken);
    }
}
