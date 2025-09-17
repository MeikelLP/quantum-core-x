using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
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
            .AddInMemoryCollection(new Dictionary<string, string?> {{"Hosting:IpAddress", "0.0.0.0"}})
            .Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(_ => config)
            .AddCoreServices(new EmptyPluginCatalog(), config)
            .AddGameServices()
            .AddSingleton(Substitute.For<IServerBase>())
            .AddSingleton(Substitute.For<IGameServer>())
            .Configure<DatabaseOptions>(HostingOptions.ModeGame, opts =>
            {
                opts.ConnectionString = "Server:abc;";
                opts.Provider = DatabaseProvider.Mysql;
            })
            .Replace(new ServiceDescriptor(typeof(IAtlasProvider), provider =>
            {
                var mock = Substitute.For<IAtlasProvider>();
                mock.GetAsync(Arg.Any<IWorld>()).Returns(info => new[]
                {
                    new Map(provider.GetRequiredService<IMonsterManager>(),
                        provider.GetRequiredService<IAnimationManager>(),
                        provider.GetRequiredService<ICacheManager>(), info.Arg<IWorld>(),
                        provider.GetRequiredService<ILogger<Map>>(),
                        provider.GetRequiredService<ISpawnPointProvider>(),
                        provider.GetRequiredService<IDropProvider>(),
                        provider.GetRequiredService<IItemManager>(),
                        provider.GetRequiredService<IServerBase>(),
                        "test_map", new Coordinates(), 1024, 1024, null, provider)
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
                mock.Get(EPlayerClassGendered.NinjaFemale).Returns(new Job());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IMonsterManager), _ =>
            {
                var mock = Substitute.For<IMonsterManager>();
                mock.GetMonster(42).Returns(new MonsterData {Type = (byte)EEntityType.Monster});
                mock.GetMonsters().Returns([
                    new MonsterData {Type = (byte)EEntityType.Monster}
                ]);
                return mock;
            }, ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFileProvider>())
            .BuildServiceProvider();
        _world = ActivatorUtilities.CreateInstance<World>(services);
        ActivatorUtilities.CreateInstance<GameServer>(services); // for setting the singleton GameServer.Instance
        Task.WhenAll(services.GetServices<ILoadable>().Select(x => x.LoadAsync())).Wait();
        _world.InitAsync().Wait();

        var conn = Substitute.For<IGameConnection>();
        conn.BoundIpAddress.Returns(IPAddress.Loopback);
        var playerData = new PlayerData
        {
            Name = "TestPlayer", PlayerClass = EPlayerClassGendered.NinjaFemale, PositionX = 1, PositionY = 1
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
