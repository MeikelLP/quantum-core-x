using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumCore;
using QuantumCore.Migrator;

await Parser.Default.ParseArguments<MigrateOptions>(args).WithParsedAsync(obj => RunAsync(obj, args));

static async Task RunAsync(MigrateOptions obj, string[] args)
{
    var hostBuilder = await QuantumCoreHostBuilder.CreateHostAsync(args);
    hostBuilder.ConfigureServices(services =>
    {
        services.AddSingleton<IOptions<MigrateOptions>>(
            _ => new OptionsWrapper<MigrateOptions>(obj));
        services.AddHostedService<Migrate>();
    });

    await hostBuilder.Build().StartAsync();
}