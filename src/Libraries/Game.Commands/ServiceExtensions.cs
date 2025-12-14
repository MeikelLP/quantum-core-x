using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuantumCore.API;
using QuantumCore.Caching.Extensions;

namespace QuantumCore.Game.Commands;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameCommands(this IServiceCollection services)
    {
        services.AddQuantumCoreCaching();
        services.AddOptions<GameCommandOptions>().BindConfiguration(GameCommandOptions.ConfigSection);
        services.TryAddSingleton<ICommandManager, CommandManager>();
        return services;
    }
}