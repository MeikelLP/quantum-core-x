using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Extensions;
using Serilog;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;

namespace Game.Tests;

public class MigrateTests
{
    [Fact]
    public async Task MysqlAsync()
    {
        var container = new MySqlBuilder("mysql:9.7.2")
            .WithDatabase("game")
            .WithUsername("metin2")
            .WithPassword("metin2")
            .Build();
        await container.StartAsync(TestContext.Current.CancellationToken);
        await ExecuteMigrateAsync(DatabaseProvider.MYSQL, container.GetConnectionString());
        Assert.True(true);
    }

    [Fact]
    public async Task PostgresqlAsync()
    {
        var container = new PostgreSqlBuilder("postgres:18.4-alpine3.24")
            .WithDatabase("game")
            .WithUsername("metin2")
            .WithPassword("metin2")
            .Build();
        await container.StartAsync(TestContext.Current.CancellationToken);
        await ExecuteMigrateAsync(DatabaseProvider.POSTGRESQL, container.GetConnectionString());
        Assert.True(true);
    }

    [Fact]
    public async Task SqliteAsync()
    {
        var fileName = $"{Guid.NewGuid()}.testdb";
        await ExecuteMigrateAsync(DatabaseProvider.SQLITE, $"Data Source={fileName};");
        Assert.True(true);
    }

    private static async Task ExecuteMigrateAsync(DatabaseProvider provider, string connectionString)
    {
        var services = new ServiceCollection()
            .AddLogging(cfg =>
            {
                cfg.ClearProviders();
                cfg.AddSerilog(new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.Console()
                    .MinimumLevel.Debug()
                    .CreateLogger());
            })
            .AddSingleton(Substitute.For<IHostEnvironment>())
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddGameDatabase()
            .Configure<DatabaseOptions>(HostingOptions.MODE_GAME, opts =>
            {
                opts.ConnectionString = connectionString;
                opts.Provider = provider;
            })
            .BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await db.Database.MigrateAsync();
    }
}