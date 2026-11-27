using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Caching.Extensions;
using QuantumCore.Extensions;

namespace QuantumCore.Game.Commands;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameCommands(this IServiceCollection services)
    {
        services.AddQuantumCoreCaching();
        services.AddOptions<GameCommandOptions>().BindConfiguration(GameCommandOptions.CONFIG_SECTION);
        services.AddLoadable<ICommandManager, CommandManager>();
        return services;
    }
}