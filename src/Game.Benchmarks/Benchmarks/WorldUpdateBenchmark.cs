using System.Net;
using BenchmarkDotNet.Attributes;
using Core.Persistence.Extensions;
using Game.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Players;
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
    [Params(0, 100, 1000)] public int _mobAmount;

    [Params(0, 1, 10)] public int _playerAmount;

    private World _world = null!;
    private readonly FakeTimeProvider _timeProvider = new();
    private ServerClock _clock = null!;

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
            .AddQuantumCoreDatabase(HostingOptions.MODE_GAME)
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
                            provider.GetRequiredService<IMapAttributeProvider>(),
                            provider.GetRequiredService<IDropProvider>(),
                            provider.GetRequiredService<IItemManager>(),
                            provider.GetRequiredService<IServerBase>(),
                            "test_map", new Coordinates(), 1024, 1024, null, provider
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
                    .Range(0, _mobAmount)
                    .Select(_ =>
                        new SpawnPoint
                        {
                            Chance = 100,
                            Type = ESpawnPointType.MONSTER,
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
                mock.Get(EPlayerClassGendered.NINJA_FEMALE).Returns(new Job());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IMonsterManager), _ =>
            {
                var mock = Substitute.For<IMonsterManager>();
                mock.GetMonster(42).Returns(new MonsterData {Type = (byte)EEntityType.MONSTER});
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(TimeProvider), _ => _timeProvider, ServiceLifetime.Singleton))
            .BuildServiceProvider();
        _clock = services.GetRequiredService<ServerClock>();
        _world = ActivatorUtilities.CreateInstance<World>(services);
        ActivatorUtilities.CreateInstance<GameServer>(services); // for setting the singleton GameServer.Instance
        _world.LoadAsync().Wait();

        foreach (var i in Enumerable.Range(0, _playerAmount))
        {
            var player = new PlayerData
            {
                Name = i.ToString(), PlayerClass = EPlayerClassGendered.NINJA_FEMALE, PositionX = 1, PositionY = 1
            };
            var conn = Substitute.For<IGameConnection>();
            conn.BoundIpAddress.Returns(IPAddress.Loopback);
            var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(services, _world, player, conn);
            _world.SpawnEntity(entity);
        }

        foreach (var e in _world.GetMapAt(0, 0)!.Entities)
        {
            e?.Goto(0, 0, Tick(0).Timestamp);
        }

        _world.Update(Tick(0.2)); // spawn entities
    }

    [Benchmark]
    public void World_Tick()
    {
        _world.Update(Tick(0.2));
    }

    private TickContext Tick(double elapsedMilliseconds)
    {
        var delta = TimeSpan.FromMilliseconds(elapsedMilliseconds);
        _timeProvider.Advance(delta);
        var now = _clock.Now;
        return new TickContext(_clock, delta, now);
    }
}
