using System.Data;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Weikio.PluginFramework.Catalogs;

namespace Game.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[MediumRunJob]
public class WorldUpdateBenchmark
{
    [Params(0, 100, 1000)]
    public int MobAmount;

    [Params(0, 1, 10)]
    public int PlayerAmount;

    private World _world = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(_ => config)
            .AddCoreServices(new EmptyPluginCatalog(), config)
            .AddQuantumCoreCache()
            .AddQuantumCoreDatabase()
            .AddGameServices()
            .Replace(new ServiceDescriptor(typeof(IDbConnection), _ => new Mock<IDbConnection>().Object, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IAtlasProvider), provider =>
            {
                var mock = new Mock<IAtlasProvider>();
                mock.Setup(x => x.GetAsync(It.IsAny<IWorld>())).ReturnsAsync<IWorld, IAtlasProvider, IEnumerable<IMap>>(world => new []
                {
                    new Map(provider.GetRequiredService<IMonsterManager>(),
                        provider.GetRequiredService<IAnimationManager>(),
                        provider.GetRequiredService<ICacheManager>(), world,
                        provider.GetRequiredService<IOptions<HostingOptions>>(),
                        provider.GetRequiredService<ILogger<Map>>(),
                        provider.GetRequiredService<ISpawnPointProvider>(), "test_map", 0, 0, 1024, 1024)
                });
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ =>
            {
                var mock = new Mock<ICacheManager>();
                mock.Setup(x => x.Keys("maps:*")).ReturnsAsync(new []{"maps:test_map"});
                mock.Setup(x => x.Subscribe()).Returns(new Mock<IRedisSubscriber>().Object);
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ISpawnPointProvider), _ =>
            {
                var mock = new Mock<ISpawnPointProvider>();
                mock.Setup(x => x.GetSpawnPointsForMap("test_map")).ReturnsAsync(Enumerable
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
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IJobManager), _ =>
            {
                var mock = new Mock<IJobManager>();
                mock.Setup(x => x.Get(1)).Returns(new Job());
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IMonsterManager), _ =>
            {
                var mock = new Mock<IMonsterManager>();
                mock.Setup(x => x.GetMonster(42)).Returns(new MonsterData
                {
                    Type = (byte)EEntityType.Monster
                });
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .BuildServiceProvider();
        _world = ActivatorUtilities.CreateInstance<World>(services);
        ActivatorUtilities.CreateInstance<GameServer>(services); // for setting the singleton GameServer.Instance
        _world.Load().Wait();

        foreach (var i in Enumerable.Range(0, PlayerAmount))
        {
            var player = new Player
            {
                Name = i.ToString(),
                PlayerClass = 1,
                PositionX = 1,
                PositionY = 1
            };
            var connMock = new Mock<IGameConnection>();
            var conn = connMock.Object;
            var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(services, _world, player, conn);
            _world.SpawnEntity(entity).AsTask().Wait();
        }
        foreach (var e in _world.GetMapAt(0, 0).GetEntities())
        {
            e.Goto(0, 0).Wait();
        }
    }

    [Benchmark]
    public async Task World_Tick()
    {
        await _world.Update(0.2);
    }
}
