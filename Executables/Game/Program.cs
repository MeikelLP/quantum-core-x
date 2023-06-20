﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCore;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.ConfigureAppConfiguration(cfg =>
{
    cfg.AddJsonFile("data/jobs.json");
    cfg.AddTomlFile("data/shops.toml", true);
    cfg.AddTomlFile("data/groups.toml", true);
    cfg.AddTomlFile("settings.toml");
});
hostBuilder.ConfigureServices(services =>
{
    services.AddGameServices();
    services.AddQuantumCoreDatabase();
    services.AddQuantumCoreCache();
    services.AddHostedService<GameServer>();
});

await QuantumCoreHostBuilder.RunAsync<Program>(hostBuilder.Build());
