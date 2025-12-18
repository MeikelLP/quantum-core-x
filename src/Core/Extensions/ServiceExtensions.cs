using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API.PluginTypes;
using QuantumCore.Networking;
using Serilog;
using Serilog.Events;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Microsoft.DependencyInjection;

namespace QuantumCore.Extensions;

public static class ServiceExtensions
{
    private const string MESSAGE_TEMPLATE = "[{Timestamp:HH:mm:ss.fff}][{Level:u3}]{Message:lj} " +
                                           "{NewLine:1}{Exception:1}";

    /// <summary>
    /// Used to register a packet provider per application type.
    /// The application types might have duplicate packet definitions (by header) but they still might be handled
    /// differently. Thus multiple packet providers may be registered if necessary with each registered as a keyed
    /// service.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mode"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddPacketProvider<T>(this IServiceCollection services, string mode)
        where T : class, IPacketLocationProvider
    {
        services.AddKeyedSingleton<IPacketLocationProvider, T>(mode);
        services.AddKeyedSingleton<IPacketReader, PacketReader>(mode);
        services.AddKeyedSingleton<IPacketManager>(mode, (provider, key) =>
        {
            var packetLocationProvider = provider.GetRequiredKeyedService<IPacketLocationProvider>(key);
            var assemblies = packetLocationProvider.GetPacketAssemblies();
            var packetTypes = assemblies.SelectMany(x => x.ExportedTypes)
                .Where(x => x.IsAssignableTo(typeof(IPacketSerializable)) &&
                            x.GetCustomAttribute<PacketAttribute>()?.Direction.HasFlag(EDirection.INCOMING) == true)
                .OrderBy(x => x.FullName)
                .ToArray();
            var handlerTypes = assemblies.SelectMany(x => x.ExportedTypes)
                .Where(x =>
                    x.IsAssignableTo(typeof(IPacketHandler)) &&
                    x is { IsClass: true, IsAbstract: false, IsInterface: false })
                .OrderBy(x => x.FullName)
                .ToArray();
            return ActivatorUtilities.CreateInstance<PacketManager>(provider,
                new object[] { (IEnumerable<Type>) packetTypes, handlerTypes });
        });
        return services;
    }

    /// <summary>
    /// Services required by Auth & Game
    /// </summary>
    /// <param name="services"></param>
    /// <param name="pluginCatalog"></param>
    /// <returns></returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IPluginCatalog pluginCatalog,
        IConfiguration configuration)
    {
        services.AddCustomLogging(configuration);
        services.AddSingleton<IPacketManager>(provider =>
        {
            var packetTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetName().Name?.StartsWith("DynamicProxyGenAssembly") ==
                            false) // ignore Castle.Core proxies
                .SelectMany(x => x.ExportedTypes)
                .Where(x => x.IsAssignableTo(typeof(IPacketSerializable)) &&
                            x.GetCustomAttribute<PacketAttribute>()?.Direction.HasFlag(EDirection.INCOMING) == true)
                .OrderBy(x => x.FullName)
                .ToArray();
            var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic)
                .SelectMany(x => x.ExportedTypes)
                .Where(x =>
                    x.IsAssignableTo(typeof(IPacketHandler)) &&
                    x is { IsClass: true, IsAbstract: false, IsInterface: false })
                .OrderBy(x => x.FullName)
                .ToArray();
            return ActivatorUtilities.CreateInstance<PacketManager>(provider, [packetTypes, handlerTypes]);
        });
        services.AddSingleton(_ => TimeProvider.System);
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
        config.MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning);

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
        config.WriteTo.Console(outputTemplate: MESSAGE_TEMPLATE);

        // sink to rolling file
        config.WriteTo.File($"{Directory.GetCurrentDirectory()}/logs/api.log",
            fileSizeLimitBytes: 10 * 1024 * 1024,
            buffered: true,
            outputTemplate: MESSAGE_TEMPLATE);

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
