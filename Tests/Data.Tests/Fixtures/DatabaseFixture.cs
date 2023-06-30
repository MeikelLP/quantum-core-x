using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Migrator;
using Serilog;
using Testcontainers.MySql;
using Xunit;
using Xunit.Abstractions;

namespace Data.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly IMessageSink _messageSink;
    public MySqlContainer Container { get; }
    public const string UserName = "root";
    public const string Password = "supersecure.123";
    public const string Database = "mysql";

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
        var provider = new ServiceCollection()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(_messageSink)
                    .CreateLogger());
            })
            .Configure<MigrateOptions>(opts =>
            {
                opts.Password = Password;
                opts.User = UserName;
                opts.Port = Container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
            })
            .BuildServiceProvider();
        var migrator = ActivatorUtilities.CreateInstance<Migrate>(provider);

        await migrator.StartAsync(default);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}