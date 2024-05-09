using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Extensions;
using Serilog;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace Game.Tests;

public class MigrateTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public MigrateTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Mysql()
    {
        var container = new MySqlBuilder()
            .WithDatabase("game")
            .WithUsername("metin2")
            .WithPassword("metin2")
            .Build();
        await container.StartAsync();
        await ExecuteMigrate(DatabaseProvider.Mysql, container.GetConnectionString());
        Assert.True(true);
    }

    [Fact]
    public async Task Postgresql()
    {
        var container = new PostgreSqlBuilder()
            .WithDatabase("game")
            .WithUsername("metin2")
            .WithPassword("metin2")
            .Build();
        await container.StartAsync();
        await ExecuteMigrate(DatabaseProvider.Postgresql, container.GetConnectionString());
        Assert.True(true);
    }

    [Fact]
    public async Task Sqlite()
    {
        var fileName = $"{Guid.NewGuid()}.db";
        await ExecuteMigrate(DatabaseProvider.Sqlite, $"Data Source={fileName};");
        File.Delete(fileName);
        Assert.True(true);
    }

    private async Task ExecuteMigrate(DatabaseProvider provider, string connectionString)
    {
        var services = new ServiceCollection()
            .AddLogging(cfg =>
            {
                cfg.ClearProviders();
                cfg.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(_testOutputHelper)
                    .WriteTo.Console()
                    .MinimumLevel.Debug()
                    .CreateLogger());
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddGameDatabase()
            .Configure<DatabaseOptions>(opts =>
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
