using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore.API.PluginTypes;
using QuantumCore.Extensions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace QuantumCore;

public static class QuantumCoreHostBuilder
{
    public static async Task<HostApplicationBuilder> CreateHostAsync(string[] args)
    {
        // workaround for https://github.com/dotnet/project-system/issues/3619
        var assemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (!string.IsNullOrWhiteSpace(assemblyPath))
        {
            // may be null in single file deployment
            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath)!);
        }

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


        var host = new HostApplicationBuilder(args);
        host.Services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
        host.Services.AddCoreServices(pluginCatalog, host.Configuration);

        var serviceCollectionPluginTypes = pluginCatalog.GetPlugins()
            .FindAll(x => typeof(IServiceCollectionPlugin).IsAssignableFrom(x.Type))
            .Select(x => x.Type)
            .ToArray();
        foreach (var serviceCollectionPluginType in serviceCollectionPluginTypes)
        {
            try
            {
                var serviceCollectionPlugin =
                    (IServiceCollectionPlugin) Activator.CreateInstance(serviceCollectionPluginType)!;
                serviceCollectionPlugin.ModifyServiceCollection(host.Services);
            }
            catch (Exception e)
            {
                // The application will crash / not start if a service plugin throws an exception
                // this is by design. They shall only modify the services and not have side effects
                Console.WriteLine(e);
                throw;
            }
        }

        return host;
    }

    public static async Task RunAsync<T>(IHost host)
    {
        await Task.WhenAll(host.Services.GetRequiredService<IEnumerable<IPluginCatalog>>()
            .Select(x => x.Initialize()));
        var pluginExecutor = host.Services.GetRequiredService<PluginExecutor>();
        var logger = host.Services.GetRequiredService<ILogger<T>>();
        await pluginExecutor.ExecutePlugins<ISingletonPlugin>(logger, x => x.InitializeAsync());
        await host.RunAsync();
    }
}