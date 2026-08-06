using Auth.Tests.Extensions;
using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore;
using QuantumCore.Auth.Persistence;
using QuantumCore.Auth.Persistence.Extensions;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auth.Tests;

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
            .AddQuantumCoreTestLogger()
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddAuthDatabase()
            .Configure<DatabaseOptions>(HostingOptions.MODE_AUTH, opts =>
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