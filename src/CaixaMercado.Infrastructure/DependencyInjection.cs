using CaixaMercado.Infrastructure.Persistence;
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

        return services;
    }
}
