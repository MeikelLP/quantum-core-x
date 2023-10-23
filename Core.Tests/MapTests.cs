using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class MapTests
{
    private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();
    private readonly Map _map;
    private readonly IWorld _world;

    public MapTests(ITestOutputHelper testOutputHelper)
    {
        var provider = new ServiceCollection()
            .AddSingleton<IMonsterManager>(_ =>
            {
                var mock = new Mock<IMonsterManager>();
                mock.Setup(x => x.GetMonster(It.IsAny<uint>())).Returns<uint>((id) => new MonsterData
                {
                    Id = id,
                    TranslatedName = "TestMonster"
                });
                return mock.Object;
            })
            .AddSingleton<IAnimationManager>(_ => new Mock<IAnimationManager>().Object)
            .AddSingleton<ICacheManager>(_ =>
            {
                var mock = new Mock<ICacheManager>();
                mock.Setup(x => x.Subscribe()).Returns(new Mock<IRedisSubscriber>().Object);
                return mock.Object;
            })
            .AddSingleton<PluginExecutor>()
            .AddSingleton<IItemManager>(_ => new Mock<IItemManager>().Object)
            .AddSingleton<IAtlasProvider>(_ =>
            {
                var mock = new Mock<IAtlasProvider>();
                mock.Setup(x => x.GetAsync(It.IsAny<IWorld>())).ReturnsAsync((IWorld _) => new[] { _map }!);
                return mock.Object;
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddSingleton<ISpawnGroupProvider>(_ =>
            {
                var mock = new Mock<ISpawnGroupProvider>();
                mock.Setup(x => x.GetSpawnGroupsAsync()).ReturnsAsync(() => new[]
                {
                    new SpawnGroup
                    {
                        Id = 101,
                        Name = "TestGroup1",
                        Leader = 101,
                        Members =
                        {
                            new SpawnMember{Id = 101},
                            new SpawnMember{Id = 101}
                        }
                    }
                });
                mock.Setup(x => x.GetSpawnGroupCollectionsAsync()).ReturnsAsync(() => new []
                {
                    // equal items but only one will be spawned
                    new SpawnGroupCollection
                    {
                        Id = 101,
                        Name = "TestGroupCollection",
                        Groups = {
                            new SpawnGroupCollectionMember
                            {
                                Id = 101, Amount = 1
                            }
                        }
                    },
                    new SpawnGroupCollection
                    {
                        Id = 101,
                        Name = "TestGroupCollection",
                        Groups = {
                            new SpawnGroupCollectionMember
                            {
                                Id = 101, Amount = 1
                            }
                        }
                    }
                });
                return mock.Object;
            })
            .AddSingleton<IWorld, World>()
            .AddSingleton<ISpawnPointProvider>(_ =>
            {
                var mock = new Mock<ISpawnPointProvider>();
                mock.Setup(x => x.GetSpawnPointsForMap(It.IsAny<string>())).Returns(() => Task.FromResult(_spawnPoints));
                return mock.Object;
            })
            .AddOptions<HostingOptions>().Services
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).CreateLogger());
            })
            .BuildServiceProvider();
        var monsterManager = provider.GetRequiredService<IMonsterManager>();
        var animationManager = provider.GetRequiredService<IAnimationManager>();
        var cacheManager = provider.GetRequiredService<ICacheManager>();
        var spawnPointProvider = provider.GetRequiredService<ISpawnPointProvider>();
        var options = provider.GetRequiredService<IOptions<HostingOptions>>();
        var logger = provider.GetRequiredService<ILogger<MapTests>>();
        _world = provider.GetRequiredService<IWorld>();
        _map = new Map(monsterManager, animationManager, cacheManager, _world, options, logger, spawnPointProvider,
            "Test", 0, 0, 4096, 4096);
    }

    [Fact]
    public async Task Spawn_SingleEntity()
    {
        _spawnPoints = new[]
        {
            new SpawnPoint
            {
                Type = ESpawnPointType.Monster,
                Monster = 101,
                X = 500,
                Y = 500
            }
        };
        await _world.Load();

        _map.GetEntities().Should().HaveCount(1);
        var entity = _map.GetEntities()[0];
        var mob = entity.Should().BeOfType<MonsterEntity>().Subject;
        mob.Proto.Id.Should().Be(101);
    }

    [Fact]
    public async Task Spawn_Group()
    {
        _spawnPoints = new[]
        {
            new SpawnPoint
            {
                Type = ESpawnPointType.Group,
                Monster = 101,
                X = 500,
                Y = 500
            }
        };
        await _world.Load();

        _map.GetEntities().Should().HaveCount(3);
        var mobs = _map.GetEntities().Should().AllBeOfType<MonsterEntity>().Subject;
        mobs.Should().AllSatisfy(x => x.Proto.Id.Should().Be(101));
    }

    [Fact]
    public async Task Spawn_GroupCollection()
    {
        _spawnPoints = new[]
        {
            new SpawnPoint
            {
                Type = ESpawnPointType.GroupCollection,
                Monster = 101,
                X = 500,
                Y = 500
            }
        };
        await _world.Load();

        _map.GetEntities().Should().HaveCount(3);
        var mobs = _map.GetEntities().Should().AllBeOfType<MonsterEntity>().Subject;
        mobs.Should().AllSatisfy(x => x.Proto.Id.Should().Be(101));
    }
}
