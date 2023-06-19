using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Logging.Enrichers;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game;
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using QuantumCore.Game.World;
using Serilog;
using Weikio.PluginFramework.Abstractions;

namespace QuantumCore.Extensions;

public static class ServiceExtensions
{
    private const string MessageTemplate = "[{Timestamp:HH:mm:ss.fff}][{Level:u3}][{ProcessName:u5}|{MachineName}:" +
                                           "{EnvironmentUserName}]{Caller} >> {Message:lj} " +
                                           "{NewLine:1}{Exception:1}";

    public static IServiceCollection AddDatabase(this IServiceCollection services, string mode)
    {
        services.AddScoped<IDbConnection>(provider =>
        {
            GeneralOptions options = mode == "game"
                ? provider.GetRequiredService<IOptions<GameOptions>>().Value
                : provider.GetRequiredService<IOptions<AuthOptions>>().Value;
            return new MySqlConnection(mode == "game" ? options.GameString : options.AccountString);
        });
        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services, IPluginCatalog pluginCatalog)
    {
        // TODO improve
        services.AddOptions<GeneralOptions>().Configure<IConfiguration>((options, config) =>
        {
            options.AccountDatabase ??= config.GetValue<string>("account-database");
            options.AccountDatabaseHost = config.GetValue<string>("account-database-host");
            options.AccountDatabaseUser = config.GetValue<string>("account-database-user");
            options.AccountDatabasePassword = config.GetValue<string>("account-database-password");
            options.GameDatabase ??= config.GetValue<string>("game-database");
            options.GameDatabaseHost = config.GetValue<string>("game-database-host");
            options.GameDatabaseUser = config.GetValue<string>("game-database-user");
            options.GameDatabasePassword = config.GetValue<string>("game-database-password");
            options.RedisHost = config.GetValue<string>("redis-host");
            options.RedisPort = config.GetValue<int>("redis-port");
            config.Bind(options);
        });
        services.AddOptions<AuthOptions>().Configure<IConfiguration>((options, config) =>
        {
            options.AccountDatabase ??= config.GetValue<string>("account-database");
            options.AccountDatabaseHost = config.GetValue<string>("account-database-host");
            options.AccountDatabaseUser = config.GetValue<string>("account-database-user");
            options.AccountDatabasePassword = config.GetValue<string>("account-database-password");
            options.GameDatabase ??= config.GetValue<string>("game-database");
            options.GameDatabaseHost = config.GetValue<string>("game-database-host");
            options.GameDatabaseUser = config.GetValue<string>("game-database-user");
            options.GameDatabasePassword = config.GetValue<string>("game-database-password");
            options.RedisHost = config.GetValue<string>("redis-host");
            options.RedisPort = config.GetValue<int>("redis-port");
            config.Bind(options);
        });
        services.AddOptions<GameOptions>().Configure<IConfiguration>((options, config) =>
        {
            options.AccountDatabase ??= config.GetValue<string>("account-database");
            options.AccountDatabaseHost = config.GetValue<string>("account-database-host");
            options.AccountDatabaseUser = config.GetValue<string>("account-database-user");
            options.AccountDatabasePassword = config.GetValue<string>("account-database-password");
            options.GameDatabase ??= config.GetValue<string>("game-database");
            options.GameDatabaseHost = config.GetValue<string>("game-database-host");
            options.GameDatabaseUser = config.GetValue<string>("game-database-user");
            options.GameDatabasePassword = config.GetValue<string>("game-database-password");
            options.RedisHost = config.GetValue<string>("redis-host");
            options.RedisPort = config.GetValue<int>("redis-port");
            config.Bind(options);
        });
        services.AddCustomLogging();
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<GameServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime();
        });
        services.AddSingleton<IPacketManager, DefaultPacketManager>();
        services.AddSingleton<PluginExecutor>();
        services.AddSingleton<IItemManager, ItemManager>();
        services.AddSingleton<IMonsterManager, MonsterManager>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IAnimationManager, AnimationManager>();
        services.AddSingleton<IExperienceManager, ExperienceManager>();
        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<IDatabaseManager, DatabaseManager>();
        services.AddSingleton<IChatManager, ChatManager>();
        services.AddSingleton<IQuestManager, QuestManager>();
        services.AddSingleton<ICacheManager, CacheManager>();
        services.AddSingleton<IWorld, World>();
        services.AddPluginFramework()
            .AddPluginCatalog(pluginCatalog)
            .AddPluginType<ISingletonPlugin>()
            .AddPluginType<IConnectionLifetimeListener>()
            .AddPluginType<IGameTickListener>()
            .AddPluginType<IPacketOperationListener>()
            .AddPluginType<IGameEntityLifetimeListener>();

        return services;
    }

    private static IServiceCollection AddCustomLogging(this IServiceCollection services)
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

        // add custom enricher for caller information
        config.Enrich.WithCaller();

        // sink to console
        config.WriteTo.Console(outputTemplate: MessageTemplate);

        // sink to rolling file
        config.WriteTo.RollingFile($"{Directory.GetCurrentDirectory()}/logs/api.log",
            fileSizeLimitBytes: 10 * 1024 * 1024,
            buffered: true,
            outputTemplate: MessageTemplate);

        // finally, create the logger
        services.AddLogging(x =>
        {
            x.ClearProviders();
            x.AddSerilog(config.CreateLogger());
        });
        return services;
    }
}