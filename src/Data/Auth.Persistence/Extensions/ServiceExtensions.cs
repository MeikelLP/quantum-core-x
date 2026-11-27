using Core.Persistence;
using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QuantumCore.Auth.Persistence.Extensions;

public static class ServiceExtensions
{
    internal const string MODE_AUTH = "auth";

    public static IServiceCollection AddAuthDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase(MODE_AUTH);
        services.AddDbContext<MySqlAuthDbContext>();
        services.AddDbContext<PostgresqlAuthDbContext>();
        services.AddDbContext<SqliteAuthDbContext>();
        services.AddScoped<AuthDbContext>(provider =>
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>()
                .Get(MODE_AUTH);
            return options.Provider switch
            {
                DatabaseProvider.MYSQL => provider.GetRequiredService<MySqlAuthDbContext>(),
                DatabaseProvider.POSTGRESQL => provider.GetRequiredService<PostgresqlAuthDbContext>(),
                DatabaseProvider.SQLITE => provider.GetRequiredService<SqliteAuthDbContext>(),
                _ => throw new InvalidOperationException(
                    $"Cannot create db context for out of range provider: {options.Provider}")
            };
        });
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountManager, AccountManager>();

        return services;
    }
}
