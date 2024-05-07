using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.Game.Persistence;
using Serilog;
using Testcontainers.MySql;
using Xunit.Abstractions;

namespace Game.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    public MySqlContainer Container { get; }
    public const string UserName = "root";
    public const string Password = "supersecure.123";
    public const string Database = "game";

    public DatabaseFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        Container = new MySqlBuilder()
            .WithDatabase(Database)
            .WithPassword(Password)
            .WithUsername(UserName)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        var connectionString = Container.GetConnectionString();
        var provider = new ServiceCollection()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(_messageSink)
                    .CreateLogger());
            })
            .Configure<DatabaseOptions>(opts =>
            {
                opts.ConnectionString = connectionString;
                opts.Provider = DatabaseProvider.Mysql;
            })
            .AddDbContext<MySqlGameDbContext>(cfg =>
            {
                cfg.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            })
            .BuildServiceProvider();
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MySqlGameDbContext>();
            await db.Database.MigrateAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}