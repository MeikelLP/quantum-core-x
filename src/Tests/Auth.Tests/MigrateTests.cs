using Core.Tests.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore;
using QuantumCore.Auth.Persistence;
using QuantumCore.Auth.Persistence.Extensions;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Auth.Tests;

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
        var fileName = $"{Guid.NewGuid()}.testdb";
        await ExecuteMigrate(DatabaseProvider.Sqlite, $"Data Source={fileName};");
        Assert.True(true);
    }

    private async Task ExecuteMigrate(DatabaseProvider provider, string connectionString)
    {
        var services = new ServiceCollection()
            .AddQuantumCoreTestLogger(_testOutputHelper)
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddAuthDatabase()
            .Configure<DatabaseOptions>(opts =>
            {
                opts.ConnectionString = connectionString;
                opts.Provider = provider;
            })
            .BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();
    }
}
