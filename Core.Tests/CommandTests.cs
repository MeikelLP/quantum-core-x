using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using AutoBogus;
using Bogus;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class CommandTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly ServiceProvider _services;
    private readonly IPlayerEntity _player;
    private readonly Faker<Player> _playerDataFaker;

    public CommandTests(ITestOutputHelper testOutputHelper)
    {        
        _playerDataFaker = new AutoFaker<Player>()
            .RuleFor(x => x.PositionX, _ => (int)(10 * Map.MapUnit))
            .RuleFor(x => x.PositionY, _ => (int)(26 * Map.MapUnit));
        var itemManagerMock = new Mock<IItemManager>();
        itemManagerMock.Setup(x => x.GetItem(It.IsAny<uint>())).Returns(() => new AutoFaker<ItemData>().Generate());
        var connectionMock = new Mock<IGameConnection>();
        var cacheManagerMock = new Mock<ICacheManager>();
        var redisListWrapperMock = new Mock<IRedisListWrapper<Guid>>();
        var redisSubscriberWrapperMock = new Mock<IRedisSubscriber>();
        redisListWrapperMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new[] { CommandManager.Operator_Group });
        cacheManagerMock.Setup(x => x.CreateList<Guid>(It.IsAny<string>())).Returns(redisListWrapperMock.Object);
        cacheManagerMock.Setup(x => x.Subscribe()).Returns(redisSubscriberWrapperMock.Object);
        var databaseManagerMock = new Mock<IDatabaseManager>();
        var dbMock = new Mock<IDbConnection>();
        dbMock.SetupDapperAsync(c => c.QueryAsync<Guid>(It.IsAny<string>(), null, null, null, null));
        databaseManagerMock.Setup(x => x.GetGameDatabase()).Returns(dbMock.Object);
        _services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog())
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(testOutputHelper)
                    .CreateLogger());
            })
            .Replace(new ServiceDescriptor(typeof(IItemManager), _ => itemManagerMock.Object, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ => cacheManagerMock.Object, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IDatabaseManager), _ => databaseManagerMock.Object, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IJobManager), _ => new Mock<IJobManager>().Object, ServiceLifetime.Singleton))
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddSingleton(_ => connectionMock.Object)
            .AddSingleton<IPlayerEntity, PlayerEntity>()
            .AddSingleton(_ => _playerDataFaker.Generate())
            .BuildServiceProvider();
        _commandManager = _services.GetRequiredService<ICommandManager>();
        _commandManager.Register("QuantumCore.Game.Commands", typeof(SpawnCommand).Assembly);
        _connection = _services.GetRequiredService<IGameConnection>();
        connectionMock.Setup(x => x.Player).Returns(_services.GetRequiredService<IPlayerEntity>()); // this would usually happen during char select
        _player = _services.GetRequiredService<IPlayerEntity>();
    }

    [Fact]
    public async Task ClearInventoryCommand()
    {
        await _player.Load();
        await _player.Inventory.PlaceItem(new ItemInstance
        {
            Id = Guid.NewGuid(),
            Count = 1,
            ItemId = 1
        });

        Assert.NotEmpty(_player.Inventory.Items);
        await _commandManager.Handle(_connection, "/ip");
        
        Assert.Empty(_player.Inventory.Items);
    }

    [Fact]
    public async Task CommandTeleportTo()
    {
        var world = await PrepareWorldAsync();
        
        await _player.Load();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        await world.SpawnEntity(_player);
        await world.SpawnEntity(player2);
        await player2.Move((int)(11 * Map.MapUnit), (int)(27 * Map.MapUnit));
        
        Assert.Equal((int)(10 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int)(26 * Map.MapUnit), _player.PositionY);
        
        await _commandManager.Handle(_connection, $"/tp {player2.Name}");
        
        Assert.Equal((int)(11 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int)(27 * Map.MapUnit), _player.PositionY);
    }

    [Fact]
    public async Task CommandTeleportHere()
    {
        var world = await PrepareWorldAsync();

        await _player.Load();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        await world.SpawnEntity(_player);
        await world.SpawnEntity(player2);
        await player2.Move((int)(11 * Map.MapUnit), (int)(27 * Map.MapUnit));
        
        
        Assert.Equal((int)(11 * Map.MapUnit), player2.PositionX);
        Assert.Equal((int)(27 * Map.MapUnit), player2.PositionY);
        
        await _commandManager.Handle(_connection, $"/tphere {player2.Name}");

        Assert.Equal((int)(10 * Map.MapUnit), player2.PositionX);
        Assert.Equal((int)(26 * Map.MapUnit), player2.PositionY);
    }

    [Fact]
    public async Task DebugCommand()
    {
    }

    [Fact]
    public async Task ExperienceCommand()
    {
    }

    [Fact]
    public async Task GiveItemCommand()
    {
    }

    [Fact]
    public async Task GoldCommand()
    {
    }

    [Fact]
    public async Task GotoCommand()
    {
    }

    [Fact]
    public async Task HelpCommand()
    {
    }

    [Fact]
    public async Task ICommandManager()
    {
    }

    [Fact]
    public async Task KickCommand()
    {
    }

    [Fact]
    public async Task LevelCommand()
    {
    }

    [Fact]
    public async Task LogoutCommand()
    {
    }

    [Fact]
    public async Task PhaseSelectCommand()
    {
    }

    [Fact]
    public async Task QuitCommand()
    {
    }

    [Fact]
    public async Task RestartCommands()
    {
    }

    [Fact]
    public async Task SpawnCommand()
    {
    }

    [Fact]
    public async Task StatCommand()
    {
    }

    private async Task<IWorld> PrepareWorldAsync()
    {
        if (!Directory.Exists("data")) Directory.CreateDirectory("data");
        await File.WriteAllTextAsync("data/atlasinfo.txt", "map_a2	256000	665600	6	6");
        await File.WriteAllTextAsync("settings.toml", @"maps = [""map_a2""]");
        ConfigManager.Load();
        var world = _services.GetRequiredService<IWorld>();
        await world.Load();
        return world;
    }
}