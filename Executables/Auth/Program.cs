﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCore;
using QuantumCore.Auth;
using QuantumCore.Extensions;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.ConfigureServices(services =>
{
    services.AddQuantumCoreDatabase();
    services.AddQuantumCoreCache();
    services.AddHostedService<AuthServer>();
});

await QuantumCoreHostBuilder.RunAsync<Program>(hostBuilder.Build());
