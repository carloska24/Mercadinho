using CaixaMercado.Application.Operacional.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CaixaMercado.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMercadinhoApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICatalogoApplicationService, CatalogoApplicationService>();
        services.AddScoped<IVendaApplicationService, VendaApplicationService>();

        return services;
    }
}
