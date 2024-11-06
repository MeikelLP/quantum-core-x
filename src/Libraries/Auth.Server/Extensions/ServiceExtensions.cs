using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Persistence.Extensions;
using QuantumCore.Caching.Extensions;
using QuantumCore.Extensions;

namespace QuantumCore.Auth.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddPacketProvider<AuthPacketLocationProvider>(HostingOptions.ModeAuth);
        services.AddOptions<HostingOptions>(HostingOptions.ModeAuth).BindConfiguration("Hosting");
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