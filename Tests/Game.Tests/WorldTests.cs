using System.Data;
using Game.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Caching.Extensions;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Weikio.PluginFramework.Catalogs;

namespace Game.Tests;

public class WorldTests
{

    private World _world = null!;
    private readonly PlayerEntity _playerEntity;

    public WorldTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(_ => config)
            .AddCoreServices(new EmptyPluginCatalog(), config)
            .AddQuantumCoreCaching()
            .AddGameCaching()
            .AddQuantumCoreDatabase()
            .AddGameServices()
            .Replace(new ServiceDescriptor(typeof(IDbConnection), _ => Substitute.For<IDbConnection>(), ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IAccountRepository), _ => Substitute.For<IAccountRepository>(), ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IAtlasProvider), provider =>
            {
                var mock = Substitute.For<IAtlasProvider>();
                mock.GetAsync(Arg.Any<IWorld>()).Returns(info => new []
                {
                    new Map(provider.GetRequiredService<IMonsterManager>(),
                        provider.GetRequiredService<IAnimationManager>(),
                        provider.GetRequiredService<ICacheManager>(), info.Arg<IWorld>(),
                        provider.GetRequiredService<IOptions<HostingOptions>>(),
                        provider.GetRequiredService<ILogger<Map>>(),
                        provider.GetRequiredService<ISpawnPointProvider>(),
                        provider.GetRequiredService<IDropProvider>(),
                        provider.GetRequiredService<IItemManager>(),
                        "test_map", 0, 0, 1024, 1024)
                });
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ =>
            {
                var mock = Substitute.For<ICacheManager>();
                mock.Keys("maps:*").Returns(new []{"maps:test_map"});
                mock.Subscribe().Returns(Substitute.For<IRedisSubscriber>());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ISpawnPointProvider), _ =>
            {
                var mock = Substitute.For<ISpawnPointProvider>();
                mock.GetSpawnPointsForMap("test_map").Returns(Enumerable
                    .Range(0, 1)
                    .Select(_ =>
                        new SpawnPoint
                        {
                            Chance = 100,
                            Type = ESpawnPointType.Monster,
                            Monster = 42,
                            X = 1,
                            Y = 1,
                            RangeX = 0,
                            RangeY = 0
                        }
                    )
                    .ToArray()
                );
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IJobManager), _ =>
            {
                var mock = Substitute.For<IJobManager>();
                mock.Get(1).Returns(new Job());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IMonsterManager), _ =>
            {
                var mock = Substitute.For<IMonsterManager>();
                mock.GetMonster(42).Returns(new MonsterData
                {
                    Type = (byte)EEntityType.Monster
                });
                return mock;
            }, ServiceLifetime.Singleton))
            .BuildServiceProvider();
        _world = ActivatorUtilities.CreateInstance<World>(services);
        ActivatorUtilities.CreateInstance<GameServer>(services); // for setting the singleton GameServer.Instance
        _world.Load().Wait();

        var conn = Substitute.For<IGameConnection>();
        var playerData = new PlayerData
        {
            Name = "TestPlayer",
            PlayerClass = 1,
            PositionX = 1,
            PositionY = 1
        };
        _playerEntity = ActivatorUtilities.CreateInstance<PlayerEntity>(services, _world, playerData, conn);
        _world.SpawnEntity(_playerEntity);
        _world.Update(0.2); // spawn all entities
    }

    [Fact]
    public void World_Update()
    {
        _world.Update(0.2);
        Assert.True(true);
    }

    [Fact]
    public void Player_Update()
    {
        _playerEntity.Update(1);
        Assert.True(true);
    }
}
