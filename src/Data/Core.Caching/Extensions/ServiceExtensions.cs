using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace QuantumCore.Caching.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreCaching(this IServiceCollection services)
    {
        services.AddOptions<CacheOptions>()
            .BindConfiguration("Cache")
            .ValidateDataAnnotations();
        services.TryAddSingleton<ICacheManager, CacheManager>();

        return services;
    }
}