using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QuantumCore;

namespace Core.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreDatabase(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration("Database")
            .ValidateDataAnnotations();
        services.TryAddScoped<IDbConnection>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return new MySqlConnection(options.ConnectionString);
        });

        return services;
    }
}