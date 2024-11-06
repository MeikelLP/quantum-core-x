using System.Text;
using Game.Caching.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
hostBuilder.Configuration.AddQuantumCoreDefaults();
hostBuilder.Services.AddGameServices();
hostBuilder.Services.AddGameCaching();
hostBuilder.Services.AddHostedService<GameServer>();
hostBuilder.Services.AddSingleton<IGameServer>(provider =>
    provider.GetServices<IHostedService>().OfType<GameServer>().Single());
hostBuilder.Services.AddSingleton<IServerBase>(provider =>
    provider.GetServices<IHostedService>().OfType<GameServer>().Single());

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
hostBuilder.Services.Configure<ServiceProviderOptions>(opts =>
{
    opts.ValidateOnBuild = true;
    opts.ValidateScopes = true;
});
var host = hostBuilder.Build();
await using (var scope = host.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Migrating database if necessary...");
    await db.Database.MigrateAsync();
}

await QuantumCoreHostBuilder.RunAsync<Program>(host);