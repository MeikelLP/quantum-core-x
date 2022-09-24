using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.Auth;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using Weikio.PluginFramework.Abstractions;

namespace QuantumCore
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<AuthOptions, GameOptions, MigrateOptions>(args).WithParsedAsync(obj => RunAsync(obj, args));
        }

        private static async Task RunAsync(object obj, string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime(x => x.SuppressStatusMessages = true)
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
                            throw new InvalidOperationException($"Invalid option type {obj.GetType().FullName}. This should never happen");
                    }
                })
                .Build();

            // plugins
            if (!Directory.Exists("plugins"))
            {
                Directory.CreateDirectory("plugins");
            }

            await Task.WhenAll(host.Services.GetRequiredService<IEnumerable<IPluginCatalog>>()
                .Select(x => x.Initialize()));
            var pluginExecutor = host.Services.GetRequiredService<PluginExecutor>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            await pluginExecutor.ExecutePlugins<ISingletonPlugin>(logger, x => x.InitializeAsync());
            
            await host.RunAsync();
        }
    }
}