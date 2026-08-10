using CaixaMercado.Application.Operacional.Portas;

namespace CaixaMercado.Infrastructure;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
