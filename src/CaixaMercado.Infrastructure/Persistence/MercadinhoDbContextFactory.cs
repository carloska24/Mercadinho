using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CaixaMercado.Infrastructure.Persistence;

public sealed class MercadinhoDbContextFactory : IDesignTimeDbContextFactory<MercadinhoDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "CAIXA_MERCADO_DB";

    public MercadinhoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Defina a variável de ambiente {ConnectionStringEnvironmentVariable} para executar comandos de migration.");
        }

        var options = new DbContextOptionsBuilder<MercadinhoDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsAssembly(typeof(MercadinhoDbContext).Assembly.FullName))
            .Options;

        return new MercadinhoDbContext(options);
    }
}
