using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Persistence.Extensions;
using QuantumCore.Caching.Extensions;

namespace QuantumCore.Auth.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddAuthDatabase();
        services.AddQuantumCoreCaching();
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<AuthServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });

        return services;
    }
}
