using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.Auth;
using QuantumCore.Auth.Extensions;
using QuantumCore.Auth.Persistence;
using QuantumCore.Caching;
using QuantumCore.Caching.InMemory;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

var dataDir = "data";

await Parser.Default.ParseArguments<SingleRunArgs>(args)
    .WithParsedAsync(async options =>
    {
        var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
        hostBuilder.Configuration.AddQuantumCoreDefaults();
        hostBuilder.Services.AddGameServices();
        hostBuilder.Services.AddAuthServices();
        hostBuilder.Services.AddHostedService<GameServer>();
        hostBuilder.Services.AddHostedService<AuthServer>();
        hostBuilder.Services.AddSingleton<IGameServer>(provider =>
            provider.GetServices<IHostedService>().OfType<GameServer>().Single());
        hostBuilder.Services.AddSingleton<IServerBase>(provider =>
            provider.GetServices<IHostedService>().OfType<GameServer>().Single());

        // overrides
        hostBuilder.Services.Replace(new ServiceDescriptor(typeof(IRedisStore), CacheStoreType.Shared,
            typeof(InMemoryRedisStore), ServiceLifetime.Singleton));
        hostBuilder.Services.Replace(new ServiceDescriptor(typeof(IRedisStore), CacheStoreType.Server,
            typeof(InMemoryRedisStore), ServiceLifetime.Singleton));
        hostBuilder.Services.AddSingleton<IConfigureOptions<DatabaseOptions>>(provider =>
        {
            var fileProvider = provider.GetRequiredService<IFileProvider>();
            var filePath = fileProvider.GetFileInfo("database.db").PhysicalPath;
            return new ConfigureNamedOptions<DatabaseOptions>(HostingOptions.ModeGame, opts =>
            {
                opts.Provider = DatabaseProvider.Sqlite;
                opts.ConnectionString = $"Data Source={filePath}";
            });
        });
        hostBuilder.Services.AddSingleton<IConfigureOptions<DatabaseOptions>>(provider =>
        {
            var fileProvider = provider.GetRequiredService<IFileProvider>();
            var filePath = fileProvider.GetFileInfo("database.db").PhysicalPath;
            return new ConfigureNamedOptions<DatabaseOptions>(HostingOptions.ModeAuth, opts =>
            {
                opts.Provider = DatabaseProvider.Sqlite;
                opts.ConnectionString = $"Data Source={filePath}";
            });
        });
        hostBuilder.Services.Configure<HostingOptions>(HostingOptions.ModeGame, opts => { opts.Port = 13001; });
        hostBuilder.Services.Configure<HostingOptions>(HostingOptions.ModeAuth, opts => { opts.Port = 11002; });

        var host = hostBuilder.Build();

        using var serviceScope = host.Services.CreateScope();
        var gameDb = serviceScope.ServiceProvider.GetRequiredService<GameDbContext>();
        var accountDb = serviceScope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await gameDb.Database.MigrateAsync();
        await accountDb.Database.MigrateAsync();

        await QuantumCoreHostBuilder.RunAsync<Program>(host);
    });

internal class SingleRunArgs;
