using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCore;
using QuantumCore.Auth;
using QuantumCore.Auth.Extensions;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.ConfigureServices(services =>
{
    services.AddAuthServices();
    services.AddHostedService<AuthServer>();
});

await QuantumCoreHostBuilder.RunAsync<Program>(hostBuilder.Build());
