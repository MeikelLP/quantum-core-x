using Microsoft.Extensions.DependencyInjection;
using QuantumCore;

namespace Core.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreDatabase(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration("Database")
            .ValidateDataAnnotations();

        return services;
    }
}
