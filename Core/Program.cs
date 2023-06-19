using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

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
            // workaround for https://github.com/dotnet/project-system/issues/3619
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!);
            
            // init plugins if any
            IPluginCatalog pluginCatalog = new EmptyPluginCatalog();
            if (Directory.Exists("plugins"))
            {
                pluginCatalog = new FolderPluginCatalog("plugins", cfg =>
                {
                    var sampleType = typeof(IConnectionLifetimeListener);
                    var types = sampleType.Assembly.GetExportedTypes()
                        .Where(x => x.Namespace == sampleType.Namespace)
                        .ToArray();
                    foreach (var type in types)
                    {
                        cfg.Implements(type);
                    }
                });
                await pluginCatalog.Initialize();
            }
            var host = Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime(x => x.SuppressStatusMessages = true)
                .ConfigureAppConfiguration(cfg =>
                {
                    cfg.AddJsonFile("data/jobs.json");
                    cfg.AddTomlFile("data/shops.toml", true);
                    cfg.AddTomlFile("data/groups.toml", true);
                    cfg.AddTomlFile("settings.toml");
                    switch (obj)
                    {
                        case AuthOptions:
                            cfg.AddInMemoryCollection(new Dictionary<string, string> { { "Mode", "auth" } });
                            break;
                        case GameOptions:
                            cfg.AddInMemoryCollection(new Dictionary<string, string> { { "Mode", "game" } });
                            break;
                    }
                })
                .ConfigureServices((ctx, services) =>
                {
                    switch (obj)
                    {
                        case AuthOptions auth:
                            services.AddCoreServices(pluginCatalog);
                            services.AddDatabase("auth");
                            services.AddHostedService<AuthServer>();
                            break;
                        case GameOptions game:
                            services.AddDatabase("game");
                            services.AddCoreServices(pluginCatalog);
                            services.AddHostedService<GameServer>();
                            break;
                        case MigrateOptions migrate:
                            services.AddSingleton<IOptions<MigrateOptions>>(_ => new OptionsWrapper<MigrateOptions>(migrate));
                            services.AddHostedService<Migrate>();
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid option type {obj.GetType().FullName}. This should never happen");
                    }
                    var serviceCollectionPluginTypes = pluginCatalog.GetPlugins()
                        .FindAll(x => typeof(IServiceCollectionPlugin).IsAssignableFrom(x.Type))
                        .Select(x => x.Type)
                        .ToArray();
                    foreach (var serviceCollectionPluginType in serviceCollectionPluginTypes)
                    {
                        try
                        {
                            var serviceCollectionPlugin = (IServiceCollectionPlugin) Activator.CreateInstance(serviceCollectionPluginType)!;
                            serviceCollectionPlugin.ModifyServiceCollection(services);
                        }
                        catch (Exception e)
                        {
                            // The application will crash / not start if a service plugin throws an exception
                            // this is by design. They shall only modify the services and not have side effects
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                })
                .Build();

            await Task.WhenAll(host.Services.GetRequiredService<IEnumerable<IPluginCatalog>>()
                .Select(x => x.Initialize()));
            var pluginExecutor = host.Services.GetRequiredService<PluginExecutor>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            await pluginExecutor.ExecutePlugins<ISingletonPlugin>(logger, x => x.InitializeAsync());
            
            await host.RunAsync();
        }
    }
}