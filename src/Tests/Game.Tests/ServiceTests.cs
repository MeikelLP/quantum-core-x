using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.World;
using QuantumCore.Game;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using QuantumCore.Game.Services;
using QuantumCore.Game.Shops;
using QuantumCore.Game.World;

namespace Game.Tests;

public class ServiceTests
{
    [Fact]
    public async Task LoadableTests()
    {
        var services = new ServiceCollection()
            .AddSingleton(Substitute.For<IGameServer>())
            .AddSingleton(Substitute.For<IConfiguration>())
            .AddSingleton(Substitute.For<ICacheManager>())
            .AddSingleton(Substitute.For<IFileProvider>())
            .AddSingleton(Substitute.For<ISpawnGroupProvider>())
            .AddSingleton(Substitute.For<IParserService>())
            .AddSingleton(Substitute.For<IStructuredFileProvider>())
            .AddSingleton<PluginExecutor>()
            .AddLogging()
            .AddLoadable<IAnimationManager, AnimationManager>()
            .AddLoadable<IChatManager, ChatManager>()
            .AddLoadable<IItemManager, ItemManager>()
            .AddLoadable<IMonsterManager, MonsterManager>()
            .AddLoadable<IExperienceManager, ExperienceManager>()
            .AddLoadable<IQuestManager, QuestManager>()
            .AddLoadable<IDropProvider, DropProvider>()
            .AddLoadable<INpcShopProvider, TsvShopProvider>()
            .AddLoadable<ISkillManager, SkillManager>()
            .AddLoadable<IWorld, World>()
            .AddSingleton<ILoadable, SessionManager>()
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        ILoadable[] loadables = [];
        var delay = Task.Delay(2000, TestContext.Current.CancellationToken);
        var result = await Task.WhenAny(Task.Run(() =>
        {
            loadables = [.. services.GetServices<ILoadable>()];
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken), delay);
        if (result == delay)
        {
            Assert.Fail("Timeout reached while resolving dependencies. This is likely a dependency loop");
        }

        var itemManager = services.GetService<IItemManager>();
        var dropProvider = services.GetService<IDropProvider>();

        loadables.Should().HaveCount(11);
        itemManager.Should().NotBeNull();
        dropProvider.Should().NotBeNull();

        loadables.Should().ContainSingle(x => ReferenceEquals(x, itemManager));
        loadables.Should().ContainSingle(x => ReferenceEquals(x, dropProvider));
    }

    [Fact]
    public void InvalidRegistration()
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddLoadable<ILoadable, SessionManager>());
    }
}