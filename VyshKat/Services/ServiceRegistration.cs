using Microsoft.Extensions.DependencyInjection;

namespace VyshKat.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddVyshKat(this IServiceCollection services)
    {
        services.AddSingleton<IServiceCenter, ServiceCenter>();
        services.AddSingleton<FleetService>();
        return services;
    }
}