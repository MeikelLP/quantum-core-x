using System.Text;
using Game.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCore;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence.Extensions;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
hostBuilder.ConfigureAppConfiguration(cfg =>
{
    cfg.AddJsonFile("data/jobs.json");
    cfg.AddTomlFile("data/shops.toml", true);
    cfg.AddTomlFile("data/groups.toml", true);
});
hostBuilder.ConfigureServices(services =>
{
    services.AddGameServices();
    services.AddGameDatabase();
    services.AddGameCaching();
    services.AddHostedService<GameServer>();
});

await QuantumCoreHostBuilder.RunAsync<Program>(hostBuilder.Build());
