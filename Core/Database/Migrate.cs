using System;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumCore.Core;
using QuantumCore.Migrations;
using Serilog;

namespace QuantumCore.Database
{
    public class Migrate : IHostedService
    {
        private readonly MigrateOptions _options;
        
        public Migrate(IOptions<MigrateOptions> options)
        {
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken token)
        {
            var serviceProvider = CreateServices("account", _options.AccountString);

            await using (serviceProvider.CreateAsyncScope())
            {
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                try
                {
                    runner.MigrateUp();
                }
                catch (MissingMigrationsException)
                {
                    Log.Information("No migrations found for account database");
                }
            }

            serviceProvider = CreateServices("game", _options.GameString);
            await using (serviceProvider.CreateAsyncScope())
            {
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                try
                {
                    runner.MigrateUp();
                }
                catch (MissingMigrationsException)
                {
                    Log.Information("No migrations found for game database");
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
}