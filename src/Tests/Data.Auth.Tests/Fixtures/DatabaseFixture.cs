using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.Auth.Persistence;
using Serilog;
using Testcontainers.MySql;
using Xunit;

namespace Data.Auth.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public MySqlContainer Container { get; }
    public const string USER_NAME = "root";
    public const string PASSWORD = "supersecure.123";
    public const string DATABASE = "auth";

    public DatabaseFixture()
    {
        Container = new MySqlBuilder("mysql:9.7.2")
            .WithDatabase(DATABASE)
            .WithPassword(PASSWORD)
            .WithUsername(USER_NAME)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        var connectionString = Container.GetConnectionString();
        var provider = new ServiceCollection()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger());
            })
            .Configure<DatabaseOptions>(HostingOptions.MODE_AUTH, opts =>
            {
                opts.ConnectionString = connectionString;
                opts.Provider = DatabaseProvider.MYSQL;
            })
            .AddDbContext<MySqlAuthDbContext>(cfg =>
            {
                cfg.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            })
            .BuildServiceProvider();
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MySqlAuthDbContext>();
            await db.Database.MigrateAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}