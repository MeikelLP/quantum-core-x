using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuantumCore.API;

namespace QuantumCore.Auth.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddDbContext<MySqlAuthDbContext>();
        services.AddDbContext<PostgresqlAuthDbContext>();
        services.AddDbContext<SqliteAuthDbContext>();
        services.AddScoped<AuthDbContext>(provider =>
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>().Value;
            return options.Provider switch
            {
                DatabaseProvider.Mysql => provider.GetRequiredService<MySqlAuthDbContext>(),
                DatabaseProvider.Postgresql => provider.GetRequiredService<PostgresqlAuthDbContext>(),
                DatabaseProvider.Sqlite => provider.GetRequiredService<SqliteAuthDbContext>(),
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
