using Core.Persistence.Extensions;
using Game.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCore;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.ConfigureAppConfiguration(cfg =>
{
    cfg.AddJsonFile("data/jobs.json");
    cfg.AddTomlFile("data/shops.toml", true);
    cfg.AddTomlFile("data/groups.toml", true);
});
hostBuilder.ConfigureServices(services =>
{
    services.AddGameServices();
    services.AddQuantumCoreDatabase();
    services.AddGameCaching();
    services.AddHostedService<GameServer>();
});

var build = hostBuilder.Build();
await QuantumCoreHostBuilder.RunAsync<Program>(build);
