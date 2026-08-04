using System.Net;
using Core.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Weikio.PluginFramework.Catalogs;

namespace Game.Tests;

public class WorldTests : IAsyncLifetime
{
    private World _world = null!;
    private PlayerEntity _playerEntity = null!;
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ServerClock _clock;
    private readonly GameServer _gameServer;
    private readonly ServiceProvider _services;
    private static readonly string[] returnThis = new[] { "maps:test_map" };

    public WorldTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Hosting:IpAddress", "0.0.0.0" } })
            .Build();
        _services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(_ => config)
            .AddCoreServices(new EmptyPluginCatalog(), config)
            .AddGameServices()
            .AddSingleton(Substitute.For<IServerBase>())
            .AddSingleton(Substitute.For<IGameServer>())
            .Configure<DatabaseOptions>(HostingOptions.MODE_GAME, opts =>
            {
                opts.ConnectionString = "Server:abc;";
                opts.Provider = DatabaseProvider.MYSQL;
            })
            .Replace(new ServiceDescriptor(typeof(IAtlasProvider), provider =>
            {
                var mock = Substitute.For<IAtlasProvider>();
                mock.GetAsync(Arg.Any<IWorld>()).Returns(info => new[]
                {
                    new Map(provider.GetRequiredService<IMonsterManager>(),
                        provider.GetRequiredService<IAnimationManager>(),
                        provider.GetRequiredService<ICacheManager>(), info.Arg<IWorld>()!,
                        provider.GetRequiredService<ILogger<Map>>(),
                        provider.GetRequiredService<ISpawnPointProvider>(),
                        provider.GetRequiredService<IMapAttributeProvider>(),
                        provider.GetRequiredService<IDropProvider>(),
                        provider.GetRequiredService<IServerBase>(),
                        "test_map", new Coordinates(), 1024, 1024, null, provider)
                });
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ =>
            {
                var mock = Substitute.For<ICacheManager>();
                mock.KeysAsync("maps:*").Returns(returnThis);
                mock.Subscribe().Returns(Substitute.For<IRedisSubscriber>());
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ISpawnPointProvider), _ =>
            {
                var mock = Substitute.For<ISpawnPointProvider>();
                mock.GetSpawnPointsForMapAsync("test_map").Returns(Enumerable
                    .Range(0, 1)
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
                mock.GetMonster(42).Returns(new MonsterData { Type = (byte)EEntityType.MONSTER });
                mock.GetMonsters().Returns([
                    new MonsterData { Type = (byte)EEntityType.MONSTER }
                ]);
                return mock;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(TimeProvider), _ => _timeProvider, ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFileProvider>())
            .BuildServiceProvider();
        _clock = _services.GetRequiredService<ServerClock>();
        var server = _services.GetRequiredService<IServerBase>();
        server.Clock.Returns(_clock);
        _world = ActivatorUtilities.CreateInstance<World>(_services);
        _gameServer =
            ActivatorUtilities.CreateInstance<GameServer>(_services); // for setting the singleton GameServer.Instance
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_services.GetServices<ILoadable>().Select(x => x.LoadAsync()));
        await _world.InitAsync();

        var conn = Substitute.For<IGameConnection>();
        conn.BoundIpAddress.Returns(IPAddress.Loopback);
        conn.Server.Returns(_gameServer);
        var playerData = new PlayerData
        {
            Name = "TestPlayer", PlayerClass = EPlayerClassGendered.NINJA_FEMALE, PositionX = 1, PositionY = 1
        };
        _playerEntity = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _world, playerData, conn);
        _world.SpawnEntity(_playerEntity);
        _world.Update(Tick(0.2)); // spawn all entities
    }

    public Task DisposeAsync() => throw new NotImplementedException();

    [Fact]
    public void World_Update()
    {
        _world.Update(Tick(0.2));
        Assert.True(true);
    }

    [Fact]
    public void Player_Update()
    {
        _playerEntity.Update(Tick(1));
        Assert.True(true);
    }

    private TickContext Tick(double elapsedMilliseconds)
    {
        var delta = TimeSpan.FromMilliseconds(elapsedMilliseconds);
        _timeProvider.Advance(delta);
        var now = _clock.Now;
        return new TickContext(_clock, delta, now);
    }
}