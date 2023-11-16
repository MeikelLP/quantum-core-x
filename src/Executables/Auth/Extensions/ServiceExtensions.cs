using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Persistence.Extensions;

namespace QuantumCore.Auth.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddAuthDatabase();
        services.AddOptions<HostingOptions>("auth")
            .BindConfiguration("Auth:Hosting")
            .ValidateDataAnnotations();
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<AuthServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime();
        });

        return services;
    }
}