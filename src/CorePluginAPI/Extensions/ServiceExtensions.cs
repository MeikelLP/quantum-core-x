using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddLoadable<TService, TImplementation>(this IServiceCollection services,
        object? key = null, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : TService, ILoadable
    {
        ArgumentNullException.ThrowIfNull(services);
        if (typeof(TService) == typeof(ILoadable))
        {
            throw new ArgumentException(
                $"{nameof(TService)} should not be ILoadable because this would create a circular dependency. You should probably call services.AddSingleton<ILoadable, {typeof(TService).Name}>()",
                nameof(TService));
        }

        services.Add(new ServiceDescriptor(typeof(TService), key, typeof(TImplementation), lifetime));
        services.Add(new ServiceDescriptor(typeof(ILoadable), key,
            (provider, k) =>
            {
                var service = provider.GetRequiredKeyedService(typeof(TService), k);
                try
                {
                    return (ILoadable)service;
                }
                catch (InvalidCastException e)
                {
                    // improve error message especially for tests
                    throw new InvalidCastException($"Cannot cast type of {service.GetType()} to {typeof(TService)}. " +
                                                   $"You probably didn't use the AddLoadable extension method or are trying to replace a service with a mock that doesn't implement ILoadable.",
                        e);
                }
            }, lifetime));

        return services;
    }
}