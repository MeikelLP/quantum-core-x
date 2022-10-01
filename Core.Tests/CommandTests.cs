using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using AutoBogus;
using Bogus;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class CommandTests : IAsyncLifetime
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly ServiceProvider _services;
    private readonly IPlayerEntity _player;
    private readonly Faker<Player> _playerDataFaker;
    private readonly List<object> _sentObjects = new();

    public CommandTests(ITestOutputHelper testOutputHelper)
    {        
        _playerDataFaker = new AutoFaker<Player>()
            .RuleFor(x => x.Level, _ => (byte)1)
            .RuleFor(x => x.St, _ => (byte)1)
            .RuleFor(x => x.Dx, _ => (byte)1)
            .RuleFor(x => x.Gold, _ => (uint)0)
            .RuleFor(x => x.Experience, _ => (uint)0)
            .RuleFor(x => x.PositionX, _ => (int)(10 * Map.MapUnit))
            .RuleFor(x => x.PositionY, _ => (int)(26 * Map.MapUnit));
        var experienceManagerMock = new Mock<IExperienceManager>();
        experienceManagerMock.Setup(x => x.GetNeededExperience(It.IsAny<byte>())).Returns(1000);
        var jobManagerMock = new Mock<IJobManager>();
        jobManagerMock.Setup(x => x.Get(It.IsAny<byte>())).Returns(new Job());
        var itemManagerMock = new Mock<IItemManager>();
        itemManagerMock.Setup(x => x.GetItem(It.IsAny<uint>())).Returns<uint>(id => new AutoFaker<ItemData>()
            .RuleFor(x => x.Id, _ => id)
            .RuleFor(x => x.Size, _ => (byte)1)
            .Generate());
        var connectionMock = new Mock<IGameConnection>();
        connectionMock.Setup(x => x.Send(It.IsAny<object>())).Callback<object>(obj => _sentObjects.Add(obj));
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
            .Replace(new ServiceDescriptor(typeof(IJobManager), _ => jobManagerMock.Object, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IExperienceManager), _ => experienceManagerMock.Object, ServiceLifetime.Singleton))
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

    public async Task InitializeAsync()
    {
        await _player.Load();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ClearInventoryCommand()
    {
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
        await _commandManager.Handle(_connection, "debug_damage");

        // simple calculation just for this test
        var minAttack = _player.GetPoint(EPoints.Level) + _player.GetPoint(EPoints.St);
        var maxAttack = _player.GetPoint(EPoints.Level) + _player.GetPoint(EPoints.St);
        const int minWeapon = 0;
        const int maxWeapon = 0;
        _sentObjects.Should().ContainEquivalentOf(new ChatOutcoming { Message = $"Weapon Damage: {minWeapon}-{maxWeapon}" }, cfg => cfg.Including(x => x.Message));
        _sentObjects.Should().ContainEquivalentOf(new ChatOutcoming { Message = $"Attack Damage: {minAttack}-{maxAttack}" }, cfg => cfg.Including(x => x.Message));
    }

    [Fact]
    public async Task ExperienceSelfCommand()
    {
        await _commandManager.Handle(_connection, "/exp 500");

        _player.GetPoint(EPoints.Experience).Should().Be(500);
    }

    [Fact]
    public async Task ExperienceOtherCommand()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        await world.SpawnEntity(_player);
        await world.SpawnEntity(player2);
        
        await _commandManager.Handle(_connection, $"/exp 500 {player2.Name}");

        player2.GetPoint(EPoints.Experience).Should().Be(500);
    }

    [Fact]
    public async Task GiveSelfItemCommand()
    {
        await _commandManager.Handle(_connection, "/give $self 1 10");

        _player.Inventory.Items.Should().NotBeEmpty();
        _player.Inventory.Items.Should().ContainEquivalentOf(new ItemInstance
        {
            ItemId = 1,
            Count = 10
        }, cfg => cfg.Including(x => x.ItemId).Including(x => x.Count));
    }

    [Fact]
    public async Task GiveOtherItemCommand()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        await world.SpawnEntity(_player);
        await world.SpawnEntity(player2);
        
        await _commandManager.Handle(_connection, $"/give \"{player2.Name}\" 1 10");

        player2.Inventory.Items.Should().NotBeEmpty();
        player2.Inventory.Items.Should().ContainEquivalentOf(new ItemInstance
        {
            ItemId = 1,
            Count = 10
        }, cfg => cfg.Including(x => x.ItemId).Including(x => x.Count));
    }

    [Fact]
    public async Task GoldCommand_Self()
    {
        _player.GetPoint(EPoints.Gold).Should().Be(0);
        await _commandManager.Handle(_connection, "/gold 10");

        _player.GetPoint(EPoints.Gold).Should().Be(10);
    }

    [Fact]
    public async Task GoldCommand_Other()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        await world.SpawnEntity(_player);
        await world.SpawnEntity(player2);
        
        player2.GetPoint(EPoints.Gold).Should().Be(0);
        await _commandManager.Handle(_connection, $"/gold 10 \"{player2.Name}\"");
        player2.GetPoint(EPoints.Gold).Should().Be(10);
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