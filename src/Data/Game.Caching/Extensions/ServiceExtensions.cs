using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Caching.Extensions;

namespace Game.Caching.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameCaching(this IServiceCollection services)
    {
        services.AddQuantumCoreCaching();
        services.AddSingleton<ICachePlayerRepository, CachePlayerRepository>();

        return services;
    }
}