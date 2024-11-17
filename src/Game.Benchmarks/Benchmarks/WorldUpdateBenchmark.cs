﻿using BenchmarkDotNet.Attributes;
using Core.Persistence.Extensions;
using Game.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Caching.Extensions;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Weikio.PluginFramework.Catalogs;

namespace Game.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[MediumRunJob]
public class WorldUpdateBenchmark
{
    [Params(0, 100, 1000)] public int MobAmount;

    [Params(0, 1, 10)] public int PlayerAmount;

    private World _world = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(_ => config)
            .AddCoreServices(new EmptyPluginCatalog(), config)
            .AddQuantumCoreCaching()
            .AddGameCaching()
            .AddQuantumCoreDatabase(HostingOptions.ModeGame)
            .AddGameServices()
            .Replace(new ServiceDescriptor(typeof(IAtlasProvider), provider =>
            {
                var mock = Substitute.For<IAtlasProvider>();
                mock.GetAsync(Arg.Any<IWorld>()).Returns(
                    callInfo => new[]
                    {
                        new Map(provider.GetRequiredService<IMonsterManager>(),
                            provider.GetRequiredService<IAnimationManager>(),
                            provider.GetRequiredService<ICacheManager>(), callInfo.Arg<IWorld>(),
                            provider.GetRequiredService<ILogger<Map>>(),
                            provider.GetRequiredService<ISpawnPointProvider>(),
                            provider.GetRequiredService<IDropProvider>(),
                            provider.GetRequiredService<IItemManager>(),
                            provider.GetRequiredService<IServerBase>(),
                            "test_map", 0, 0, 1024, 1024
                        )
                    });

                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ =>
            {
                var mock = Substitute.For<ICacheManager>();
                mock.Keys("maps:*").Returns(new[] {"maps:test_map"});
                mock.Subscribe().Returns(Substitute.For<IRedisSubscriber>());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ISpawnPointProvider), _ =>
            {
                var mock = Substitute.For<ISpawnPointProvider>();
                mock.GetSpawnPointsForMap("test_map").Returns(Enumerable
                    .Range(0, MobAmount)
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
                    Type = (byte) EEntityType.Monster
                });
                return mock;
            }, ServiceLifetime.Singleton))
            .BuildServiceProvider();
        _world = ActivatorUtilities.CreateInstance<World>(services);
        ActivatorUtilities.CreateInstance<GameServer>(services); // for setting the singleton GameServer.Instance
        _world.LoadAsync().Wait();

        foreach (var i in Enumerable.Range(0, PlayerAmount))
        {
            var player = new PlayerData
            {
                Name = i.ToString(),
                PlayerClass = 1,
                PositionX = 1,
                PositionY = 1
            };
            var conn = Substitute.For<IGameConnection>();
            var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(services, _world, player, conn);
            _world.SpawnEntity(entity);
        }

        foreach (var e in _world.GetMapAt(0, 0)!.Entities)
        {
            e?.Goto(0, 0);
        }

        _world.Update(0.2); // spawn entities
    }

    [Benchmark]
    public void World_Tick()
    {
        _world.Update(0.2);
    }
}