using System.Data;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QuantumCore.API.Data;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Networking;
using Serilog;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Microsoft.DependencyInjection;

namespace QuantumCore.Extensions;

public static class ServiceExtensions
{
    private const string MessageTemplate = "[{Timestamp:HH:mm:ss.fff}][{Level:u3}]{Message:lj} " +
                                           "{NewLine:1}{Exception:1}";

    /// <summary>
    /// Services required by Auth & Game
    /// </summary>
    /// <param name="services"></param>
    /// <param name="pluginCatalog"></param>
    /// <returns></returns>
    public static IServiceCollection AddQuantumCoreDatabase(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration("Database")
            .ValidateDataAnnotations();
        services.AddSingleton<IPlayerManager, PlayerManager>();
        services.AddTransient<IDbConnection>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return new MySqlConnection(options.ConnectionString);
        });

        return services;
    }
    public static IServiceCollection AddQuantumCoreCache(this IServiceCollection services)
    {
        services.AddOptions<CacheOptions>()
            .BindConfiguration("Cache")
            .ValidateDataAnnotations();
        services.AddSingleton<ICacheManager, CacheManager>();

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services, IPluginCatalog pluginCatalog, IConfiguration configuration)
    {
        services.AddOptions<HostingOptions>()
            .BindConfiguration("Hosting")
            .ValidateDataAnnotations();
        services.AddCustomLogging(configuration);
        services.AddSingleton<IPacketManager>(provider =>
        {
            var packetTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes)
                .Where(x => x.IsAssignableTo(typeof(IPacketSerializable)) &&
                            x.GetCustomAttribute<PacketAttribute>()?.Direction.HasFlag(EDirection.Incoming) == true)
                .OrderBy(x => x.FullName)
                .ToArray();
            var handlerTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes)
                .Where(x =>
                    x.IsAssignableTo(typeof(IPacketHandler)) &&
                    x is { IsClass: true, IsAbstract: false, IsInterface: false })
                .OrderBy(x => x.FullName)
                .ToArray();
            return ActivatorUtilities.CreateInstance<PacketManager>(provider, new object[] { (IEnumerable<Type>)packetTypes, handlerTypes });
        });
        services.AddSingleton<IPacketReader, PacketReader>();
        services.AddSingleton<PluginExecutor>();
        services.AddPluginFramework()
            .AddPluginCatalog(pluginCatalog)
            .AddPluginType<ISingletonPlugin>()
            .AddPluginType<IConnectionLifetimeListener>()
            .AddPluginType<IGameTickListener>()
            .AddPluginType<IPacketOperationListener>()
            .AddPluginType<IGameEntityLifetimeListener>();

        return services;
    }

    private static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new LoggerConfiguration();

        // add minimum log level for the instances
#if DEBUG
        config.MinimumLevel.Verbose();
#else
            config.MinimumLevel.Information();
#endif

        // add destructuring for entities
        config.Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumCollectionCount(10)
            .Destructure.ToMaximumStringLength(100);

        // add environment variable
        config.Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName();

        // add process information
        config.Enrich.WithProcessId()
            .Enrich.WithProcessName();

        // add assembly information
        // TODO: uncomment if needed
        /* config.Enrich.WithAssemblyName() // {AssemblyName}
            .Enrich.WithAssemblyVersion(true) // {AssemblyVersion}
            .Enrich.WithAssemblyInformationalVersion(); */

        // add exception information
        config.Enrich.WithExceptionData();

        // sink to console
        config.WriteTo.Console(outputTemplate: MessageTemplate);

        // sink to rolling file
        config.WriteTo.RollingFile($"{Directory.GetCurrentDirectory()}/logs/api.log",
            fileSizeLimitBytes: 10 * 1024 * 1024,
            buffered: true,
            outputTemplate: MessageTemplate);

        config.ReadFrom.Configuration(configuration);

        // finally, create the logger
        services.AddLogging(x =>
        {
            x.ClearProviders();
            x.AddSerilog(config.CreateLogger());
        });
        return services;
    }
}
