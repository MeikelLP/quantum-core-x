﻿using Core.Tests.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
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
                var mock = Substitute.For<IMonsterManager>();
                mock.GetMonster(Arg.Any<uint>()).Returns(call => new MonsterData
                {
                    Id = call.Arg<uint>(),
                    TranslatedName = "TestMonster"
                });
                return mock;
            })
            .AddSingleton<IAnimationManager>(_ => Substitute.For<IAnimationManager>())
            .AddSingleton<ICacheManager>(_ =>
            {
                var mock = Substitute.For<ICacheManager>();
                mock.Subscribe().Returns(Substitute.For<IRedisSubscriber>());
                mock.Keys(Arg.Any<string>()).Returns(_ => Array.Empty<string>());
                return mock;
            })
            .AddSingleton<PluginExecutor>()
            .AddSingleton<IItemManager>(_ => Substitute.For<IItemManager>())
            .AddSingleton<IAtlasProvider>(_ =>
            {
                var mock = Substitute.For<IAtlasProvider>();
                mock.GetAsync(Arg.Any<IWorld>()).Returns(_ => new[] {_map}!);
                return mock;
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddSingleton<ISpawnGroupProvider>(_ =>
            {
                var mock = Substitute.For<ISpawnGroupProvider>();
                mock.GetSpawnGroupsAsync().Returns(_ => new[]
                {
                    new SpawnGroup
                    {
                        Id = 101,
                        Name = "TestGroup1",
                        Leader = 101,
                        Members =
                        {
                            new SpawnMember {Id = 101},
                            new SpawnMember {Id = 101}
                        }
                    }
                });
                mock.GetSpawnGroupCollectionsAsync().Returns(_ => new[]
                {
                    // equal items but only one will be spawned
                    new SpawnGroupCollection
                    {
                        Id = 101,
                        Name = "TestGroupCollection",
                        Groups =
                        {
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
                        Groups =
                        {
                            new SpawnGroupCollectionMember
                            {
                                Id = 101, Amount = 1
                            }
                        }
                    }
                });
                return mock;
            })
            .AddSingleton<IWorld, World>()
            .AddSingleton<ISpawnPointProvider>(_ =>
            {
                var mock = Substitute.For<ISpawnPointProvider>();
                mock.GetSpawnPointsForMap(Arg.Any<string>()).Returns(_ => Task.FromResult(_spawnPoints));
                return mock;
            })
            .AddOptions<HostingOptions>().Services
            .AddQuantumCoreTestLogger(testOutputHelper)
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
        _world.Update(0); // spawn entities

        _map.Entities.Should().HaveCount(1);
        var entity = _map.Entities.ElementAt(0);
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
        _world.Update(0); // spawn entities

        _map.Entities.Should().HaveCount(3);
        var mobs = _map.Entities.Should().AllBeOfType<MonsterEntity>().Subject;
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
        _world.Update(0); // spawn entities

        _map.Entities.Should().HaveCount(3);
        var mobs = _map.Entities.Should().AllBeOfType<MonsterEntity>().Subject;
        mobs.Should().AllSatisfy(x => x.Proto.Id.Should().Be(101));
    }
}
