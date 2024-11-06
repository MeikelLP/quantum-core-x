using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.Auth;
using QuantumCore.Auth.Extensions;
using QuantumCore.Auth.Persistence;
using QuantumCore.Auth.Persistence.Extensions;
using QuantumCore.Extensions;
using Weikio.PluginFramework.Catalogs;

inFramework.Catalogs;

var hostBuilder = WebApplication.CreateBuilder(args);

hostBuilder.Services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
hostBuilder.Services.AddCoreServices(new EmptyPluginCatalog(), hostBuilder.Configuration);
hostBuilder.Services.AddAuthServices();
hostBuilder.Services.AddHostedService<AuthServer>();
hostBuilder.Services.AddSingleton<IServerBase>(provider =>
    provider.GetServices<IHostedService>().OfType<AuthServer>().Single());

var host = hostBuilder.Build();

// TODO security
host.MapGet("/account/{id:guid}", async Task<IResult> (Guid id, AuthDbContext db) =>
{
    var acc = await db.Accounts
        .Where(x => x.Id == id)
        .SelectAccountData()
        .FirstOrDefaultAsync();
    if (acc is null) return TypedResults.NotFound();
    return TypedResults.Json(acc);
});

await using (var scope = host.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Migrating database if necessary...");
    await db.Database.MigrateAsync();
}

await QuantumCoreHostBuilder.RunAsync<Program>(host);
