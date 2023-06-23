using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QuantumCore.Migrations;

namespace QuantumCore.Migrator;

public class Migrate : IHostedService
{
    private readonly ILogger<Migrate> _logger;
    private readonly MigrateOptions _options;
        
    public Migrate(IOptions<MigrateOptions> options, ILogger<Migrate> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken token)
    {
        var accountString = new MySqlConnectionStringBuilder
        {
            Database = "account",
            Password = _options.Password,
            Port = _options.Port,
            UserID = _options.User,
            Server = _options.Host
        };
        var serviceProvider = CreateServices("account", accountString.ToString());

        await using (serviceProvider.CreateAsyncScope())
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            try
            {
                runner.MigrateUp();
            }
            catch (MissingMigrationsException)
            {
                _logger.LogInformation("No migrations found for account database");
            }
        }

        var gameString = new MySqlConnectionStringBuilder
        {
            Database = "game",
            Password = _options.Password,
            Port = _options.Port,
            UserID = _options.User,
            Server = _options.Host
        };
        serviceProvider = CreateServices("game", gameString.ToString());
        await using (serviceProvider.CreateAsyncScope())
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            try
            {
                runner.MigrateUp();
            }
            catch (MissingMigrationsException)
            {
                _logger.LogInformation("No migrations found for game database");
            }
        }
    }

    private IServiceProvider CreateServices(string tag, string connectionString)
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddMySql5()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateAccountTable).Assembly).For.Migrations())
            .Configure<RunnerOptions>(opt =>
            {
                opt.Tags = new[] {tag};
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .Configure<FluentMigratorLoggerOptions>(opt =>
            {
                opt.ShowSql = _options.Debug;
            })
            .BuildServiceProvider();
    }

    public Task StopAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}