using Core.Persistence.Extensions;
using Game.Caching.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Extensions;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Persistence.Extensions;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using QuantumCore.Game.Services;
using QuantumCore.Game.Shops;

namespace QuantumCore.Game.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameServices(this IServiceCollection services)
    {
        services.AddPacketProvider<GamePacketLocationProvider>(HostingOptions.MODE_GAME);
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<GameServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });
        services.AddGameDatabase();
        services.AddGameCaching();
        services.AddGameCommands();
        services.AddQuantumCoreDatabase(HostingOptions.MODE_GAME);
        services.AddOptions<AuthOptions>().BindConfiguration("Auth");
        services.AddOptions<GameOptions>().BindConfiguration("Game");
        services.AddOptions<HostingOptions>(HostingOptions.MODE_GAME).BindConfiguration("Hosting");
        services.AddHttpClient("", (provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<AuthOptions>>().Value;
            http.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddLoadable<IAnimationManager, AnimationManager>();
        services.AddLoadable<IChatManager, ChatManager>();
        services.AddLoadable<IItemManager, ItemManager>();
        services.AddLoadable<IMonsterManager, MonsterManager>();
        services.AddLoadable<IExperienceManager, ExperienceManager>();
        services.AddLoadable<IQuestManager, QuestManager>();
        services.AddLoadable<IDropProvider, DropProvider>();
        services.AddLoadable<INpcShopProvider, TsvShopProvider>();
        services.AddLoadable<ISkillManager, SkillManager>();
        services.AddLoadable<IWorld, World.World>();
        services.AddSingleton<ILoadable, SessionManager>();
        services.AddSingleton<IGuildExperienceManager, GuildExperienceManager>();
        services.AddSingleton<IParserService, ParserService>();
        services.AddSingleton<ISpawnGroupProvider, SpawnGroupProvider>();
        services.AddSingleton<ISpawnPointProvider, SpawnPointProvider>();
        services.AddSingleton<IMapAttributeProvider, MapAttributeProvider>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IStructuredFileProvider, StructuredFileProvider>();
        services.AddSingleton<IAtlasProvider, AtlasProvider>();
        services.AddScoped<IGuildManager, GuildManager>();
        services.AddScoped<IPlayerFactory, PlayerFactory>();
        services.AddScoped<IPlayerManager, PlayerManager>();

        return services;
    }
}