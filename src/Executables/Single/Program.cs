using Core.Persistence.Extensions;
using Game.Caching.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuantumCore;
using QuantumCore.Auth;
using QuantumCore.Auth.Extensions;
using QuantumCore.Auth.Persistence;
using QuantumCore.Caching;
using QuantumCore.Caching.InMemory;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.Configuration.AddQuantumCoreDefaults();
hostBuilder.Services.AddGameServices();
hostBuilder.Services.AddAuthServices();
hostBuilder.Services.AddQuantumCoreDatabase();
hostBuilder.Services.AddGameCaching();
hostBuilder.Services.AddHostedService<GameServer>();
hostBuilder.Services.AddHostedService<AuthServer>();

// overrides
hostBuilder.Services.Replace(new ServiceDescriptor(typeof(IRedisStore), CacheStoreType.Shared,
    typeof(InMemoryRedisStore), ServiceLifetime.Singleton));
hostBuilder.Services.Replace(new ServiceDescriptor(typeof(IRedisStore), CacheStoreType.Server,
    typeof(InMemoryRedisStore), ServiceLifetime.Singleton));
hostBuilder.Services.Configure<DatabaseOptions>("game", opts =>
{
    opts.Provider = DatabaseProvider.Sqlite;
    opts.ConnectionString = "Data Source=data/database.db";
});
hostBuilder.Services.Configure<DatabaseOptions>("auth", opts =>
{
    opts.Provider = DatabaseProvider.Sqlite;
    opts.ConnectionString = "Data Source=data/database.db";
});
hostBuilder.Services.Configure<HostingOptions>("game", opts => { opts.Port = 13001; });
hostBuilder.Services.Configure<HostingOptions>("auth", opts => { opts.Port = 11002; });

var host = hostBuilder.Build();

using var serviceScope = host.Services.CreateScope();
var gameDb = serviceScope.ServiceProvider.GetRequiredService<GameDbContext>();
var accountDb = serviceScope.ServiceProvider.GetRequiredService<AuthDbContext>();
await gameDb.Database.MigrateAsync();
await accountDb.Database.MigrateAsync();

await QuantumCoreHostBuilder.RunAsync<Program>(host);
