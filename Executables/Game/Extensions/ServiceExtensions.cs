using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<GameServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime();
        });
        services.AddSingleton<ISpawnGroupProvider, SpawnGroupProvider>();
        services.AddSingleton<ISpawnPointProvider, SpawnPointProvider>();
        services.AddSingleton<IDropProvider, DropProvider>();
        services.AddSingleton<IItemManager, ItemManager>();
        services.AddSingleton<IMonsterManager, MonsterManager>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IAtlasProvider, AtlasProvider>();
        services.AddSingleton<IAnimationManager, AnimationManager>();
        services.AddSingleton<IExperienceManager, ExperienceManager>();
        services.AddSingleton<ICommandManager, CommandManager>();
        services.AddSingleton<IChatManager, ChatManager>();
        services.AddSingleton<IQuestManager, QuestManager>();
        services.AddSingleton<IWorld, World.World>();

        return services;
    }
}
