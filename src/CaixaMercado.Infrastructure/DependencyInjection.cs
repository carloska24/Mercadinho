using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Infrastructure.Health;
using CaixaMercado.Infrastructure.Persistence;
using CaixaMercado.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CaixaMercado.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMercadinhoPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mercadinho");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("A connection string 'Mercadinho' não foi configurada.");

        services.AddDbContext<MercadinhoDbContext>(options =>
            options.UseNpgsql(connectionString, postgres =>
            {
                postgres.MigrationsAssembly(typeof(MercadinhoDbContext).Assembly.FullName);
                postgres.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null);
            }));

        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddHealthChecks()
            .AddCheck<PostgreSqlHealthCheck>("postgresql", tags: ["ready"]);

        return services;
    }
}
