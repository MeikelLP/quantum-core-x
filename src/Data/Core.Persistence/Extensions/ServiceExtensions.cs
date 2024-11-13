using Microsoft.Extensions.DependencyInjection;
using QuantumCore;

namespace Core.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreDatabase(this IServiceCollection services, string mode)
    {
        services.AddOptions<DatabaseOptions>(mode)
            .BindConfiguration("Database")
            .ValidateDataAnnotations();

        return services;
    }
}