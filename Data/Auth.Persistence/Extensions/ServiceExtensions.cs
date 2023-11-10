using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.API.Data;

namespace QuantumCore.Auth.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IAccountManager, AccountManager>();

        return services;
    }
}
