using CaixaMercado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CaixaMercado.Infrastructure.Tests;

public sealed class PostgreSqlMigrationTests
{
    [PostgreSqlFact]
    public async Task Migracoes_devem_ser_aplicadas_em_postgresql_real()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async databaseConnectionString =>
        {
            var options = new DbContextOptionsBuilder<MercadinhoDbContext>()
                .UseNpgsql(databaseConnectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(MercadinhoDbContext).Assembly.FullName))
                .Options;

            await using var context = new MercadinhoDbContext(options);
            await context.Database.MigrateAsync();

            var expected = context.Database.GetMigrations().ToArray();
            var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
            var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();

            Assert.NotEmpty(expected);
            Assert.Equal(expected, applied);
            Assert.Empty(pending);
            Assert.True(await context.Database.CanConnectAsync());
            Assert.Equal(0, await context.Produtos.CountAsync());
            Assert.Equal(0, await context.Vendas.CountAsync());
            Assert.Equal(0, await context.ItensVenda.CountAsync());
        });
    }
}
