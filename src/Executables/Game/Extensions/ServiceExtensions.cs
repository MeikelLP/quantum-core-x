using Game.Caching.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Persistence.Extensions;
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
                .WithScopedLifetime();
            scan.FromAssemblyOf<GameServer>()
                .AddClasses(classes => classes.AssignableTo<ILoadable>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });
        services.AddGameDatabase();
        services.AddGameCaching();
        services.AddGameCommands();
        services.AddOptions<AuthOptions>().BindConfiguration("Auth");
        services.AddOptions<GameOptions>().BindConfiguration("Game");
        services.AddHttpClient("", (provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<AuthOptions>>().Value;
            http.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddScoped<IPlayerManager, PlayerManager>();
        services.AddSingleton<IPlayerFactory, PlayerFactory>();
        services.AddSingleton<IParserService, ParserService>();
        services.AddSingleton<ISpawnGroupProvider, SpawnGroupProvider>();
        services.AddSingleton<ISpawnPointProvider, SpawnPointProvider>();
        services.AddSingleton<IDropProvider, DropProvider>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IAtlasProvider, AtlasProvider>();
        services.AddSingleton<IStructuredFileProvider, StructuredFileProvider>();
        services.AddSingleton<IAnimationManager, AnimationManager>();
        services.AddSingleton<IExperienceManager, ExperienceManager>();
        services.AddSingleton<IChatManager, ChatManager>();
        services.AddSingleton<IQuestManager, QuestManager>();
        services.AddSingleton<ISkillManager, SkillManager>();
        services.AddSingleton<ISessionManager, SessionManager>();

        services.AddSingleton<IWorld, World.World>();

        return services;
    }
}