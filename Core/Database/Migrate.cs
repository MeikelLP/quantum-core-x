using System;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Core;
using QuantumCore.Migrations;
using Serilog;

namespace QuantumCore.Database
{
    public class Migrate : IServer
    {
        private MigrateOptions _options;
        
        public Migrate(MigrateOptions options)
        {
            _options = options;
        }

        public async Task Init()
        {
            
        }

        public async Task Start()
        {
            var serviceProvider = CreateServices("account", _options.AccountString);

            using (var scope = serviceProvider.CreateScope())
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
            using (var scope = serviceProvider.CreateScope())
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
    }
}