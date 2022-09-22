using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumCore.Auth;
using QuantumCore.Core;
using QuantumCore.Core.Logging;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using Serilog;

namespace QuantumCore
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<AuthOptions, GameOptions, MigrateOptions>(args).WithParsedAsync(RunAsync);
        }

        private static async Task RunAsync(object obj)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    switch (obj)
                    {
                        case AuthOptions auth:
                            services.AddSingleton<IOptions<AuthOptions>>(_ => new OptionsWrapper<AuthOptions>(auth));
                            services.AddCoreServices();
                            services.AddHostedService<AuthServer>();
                            break;
                        case GameOptions game:
                            services.AddSingleton<IOptions<GameOptions>>(_ => new OptionsWrapper<GameOptions>(game));
                            services.AddCoreServices();
                            services.AddHostedService<GameServer>();
                            break;
                        case MigrateOptions migrate:
                            services.AddSingleton<IOptions<MigrateOptions>>(_ => new OptionsWrapper<MigrateOptions>(migrate));
                            services.AddHostedService<Migrate>();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                })
                .Build();

            await host.RunAsync();
        }
    }
}