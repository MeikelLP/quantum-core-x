using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Caching.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreCaching(this IServiceCollection services)
    {
        services.AddOptions<CacheOptions>()
            .BindConfiguration("Cache")
            .ValidateDataAnnotations();
        RegisterKeyedRedisStore(services, CacheStoreType.Shared);
        RegisterKeyedRedisStore(services, CacheStoreType.Server);
        services.TryAddSingleton<ICacheManager, CacheManager>();

        return services;
    }

    private static void RegisterKeyedRedisStore(IServiceCollection services, CacheStoreType storeType)
    {
        services.AddKeyedSingleton<IRedisStore>(storeType, (provider, _) =>
        {
            var logger = provider.GetRequiredService<ILogger<RedisStore>>();
            var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
            return new RedisStore(storeType, logger, options);
        });
    }
}