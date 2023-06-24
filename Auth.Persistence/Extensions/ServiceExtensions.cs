using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.Auth.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddSingleton<IAccountStore, AccountStore>();

        return services;
    }
}