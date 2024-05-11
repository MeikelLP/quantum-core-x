using AutoBogus;
using Bogus;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Core.Packets;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using QuantumCore.Networking;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;
using ICommandPermissionRepository = QuantumCore.Game.Persistence.ICommandPermissionRepository;

// cannot cast MockedGameConnection to IGameConnection ???
#pragma warning disable CS8602

namespace Core.Tests;

// Custom mock instead of Mock<T> because IPacketSerializable for IGameConnection.Send<T> cannot be used as generic
// parameter
internal class MockedGameConnection : IGameConnection
{
    public readonly List<ChatOutcoming> SentMessages = new();
    public readonly List<GCPhase> SentPhases = new();
    public Guid Id { get; }
    public EPhases Phase { get; set; }
    public Task ExecuteTask { get; } = null!;

    public void Close()
    {
    }

    public void Send<T>(T packet) where T : IPacketSerializable
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (packet is ChatOutcoming chat)
        {
            SentMessages.Add(chat);
        }
        else if (packet is GCPhase phase)
        {
            SentPhases.Add(phase);
        }
    }

    public Task StartAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public IServerBase Server { get; } = null!;
    public Guid? AccountId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public IPlayerEntity? Player { get; set; }

    public bool HandleHandshake(GCHandshakeData handshake)
    {
        return true;
    }
}

public class CommandTests : IAsyncLifetime
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly ServiceProvider _services;
    private readonly IPlayerEntity _player;
    private readonly IItemManager _itemManager;
    private readonly Faker<PlayerData> _playerDataFaker;

    public CommandTests(ITestOutputHelper testOutputHelper)
    {
        _playerDataFaker = new AutoFaker<PlayerData>()
            .RuleFor(x => x.Level, _ => (byte) 1)
            .RuleFor(x => x.St, _ => (byte) 1)
            .RuleFor(x => x.Ht, _ => (byte) 1)
            .RuleFor(x => x.Dx, _ => (byte) 1)
            .RuleFor(x => x.Gold, _ => (uint) 0)
            .RuleFor(x => x.Experience, _ => (uint) 0)
            .RuleFor(x => x.PositionX, _ => (int) (10 * Map.MapUnit))
            .RuleFor(x => x.PositionY, _ => (int) (26 * Map.MapUnit));
        var monsterManagerMock = Substitute.For<IMonsterManager>();
        monsterManagerMock.GetMonster(Arg.Any<uint>()).Returns(callerInfo =>
            new AutoFaker<MonsterData>().RuleFor(x => x.Id, _ => callerInfo.Arg<uint>()).Generate());
        var experienceManagerMock = Substitute.For<IExperienceManager>();
        experienceManagerMock.GetNeededExperience(Arg.Any<byte>()).Returns(1000u);
        var jobManagerMock = Substitute.For<IJobManager>();
        jobManagerMock.Get(Arg.Any<byte>()).Returns(new Job());
        var itemManagerMock = Substitute.For<IItemManager>();
        itemManagerMock.GetItem(Arg.Any<uint>()).Returns(call => new AutoFaker<ItemData>()
            .RuleFor(x => x.Id, _ => call.Arg<uint>())
            .RuleFor(x => x.Size, _ => (byte) 1)
            .RuleFor(x => x.WearFlags, _ => (byte) EWearFlags.Weapon)
            .RuleFor(x => x.Values, _ => new List<int> {0, 0, 0, 10, 16, 0})
            .Generate());
        var cacheManagerMock = Substitute.For<ICacheManager>();
        var redisListWrapperMock = Substitute.For<IRedisListWrapper<Guid>>();
        var redisSubscriberWrapperMock = Substitute.For<IRedisSubscriber>();
        redisListWrapperMock.Range(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new[] {PermGroup.OperatorGroup});
        cacheManagerMock.Keys(Arg.Any<string>()).Returns(Array.Empty<string>());
        cacheManagerMock.CreateList<Guid>(Arg.Any<string>()).Returns(redisListWrapperMock);
        cacheManagerMock.Subscribe().Returns(redisSubscriberWrapperMock);
        _services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog(), new ConfigurationBuilder().Build())
            .AddGameServices()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(testOutputHelper)
                    .CreateLogger());
            })
            .Replace(new ServiceDescriptor(typeof(IItemRepository), _ => Substitute.For<IItemRepository>(),
                ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICommandPermissionRepository),
                _ =>
                {
                    var mock = Substitute.For<ICommandPermissionRepository>();
                    mock.GetGroupsForPlayer(Arg.Any<Guid>()).Returns([PermGroup.OperatorGroup]);
                    return mock;
                }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IPlayerRepository), _ => Substitute.For<IPlayerRepository>(),
                ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IMonsterManager), _ => monsterManagerMock, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IItemManager), _ => itemManagerMock, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), _ => cacheManagerMock, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IJobManager), _ => jobManagerMock, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(IExperienceManager), _ => experienceManagerMock,
                ServiceLifetime.Singleton))
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"maps:0", "map_a2"},
                    {"maps:1", "map_b2"}
                })
                .Build())
            .AddSingleton(Substitute.For<IDbPlayerRepository>())
            .AddSingleton<IGameConnection>(_ => new MockedGameConnection())
            .AddSingleton<IPlayerEntity, PlayerEntity>()
            .AddSingleton(_ => _playerDataFaker.Generate())
            .BuildServiceProvider();
        _itemManager = _services.GetRequiredService<IItemManager>();
        _commandManager = _services.GetRequiredService<ICommandManager>();
        _commandManager.Register("QuantumCore.Game.Commands", typeof(SpawnCommand).Assembly);
        _connection = _services.GetRequiredService<IGameConnection>();
        _player = _services.GetRequiredService<IPlayerEntity>();
        _connection.Player = _player;
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
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);
        world.Update(0); // spawn entities
        player2.Move((int) (11 * Map.MapUnit), (int) (27 * Map.MapUnit));

        Assert.Equal((int) (10 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int) (26 * Map.MapUnit), _player.PositionY);

        await _commandManager.Handle(_connection, $"/tp \"{player2.Name}\"");

        Assert.Equal((int) (11 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int) (27 * Map.MapUnit), _player.PositionY);
    }

    [Fact]
    public async Task CommandTeleportHere()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);
        world.Update(0); // spawn entities
        player2.Move((int) (11 * Map.MapUnit), (int) (27 * Map.MapUnit));


        Assert.Equal((int) (11 * Map.MapUnit), player2.PositionX);
        Assert.Equal((int) (27 * Map.MapUnit), player2.PositionY);

        await _commandManager.Handle(_connection, $"/tphere \"{player2.Name}\"");

        Assert.Equal((int) (10 * Map.MapUnit), player2.PositionX);
        Assert.Equal((int) (26 * Map.MapUnit), player2.PositionY);
    }

    [Fact]
    public async Task DebugCommand()
    {
        var item = new ItemInstance {ItemId = 1, Count = 1};
        var wearSlot = _player.Inventory.EquipmentWindow.GetWearPosition(_itemManager, item.ItemId);

        _player.SetItem(item, (byte) WindowType.Inventory, (ushort) wearSlot);

        await _commandManager.Handle(_connection, "debug_damage");
        // simple calculation just for this test
        var minAttack = _player.GetPoint(EPoints.MinAttackDamage);
        var maxAttack = _player.GetPoint(EPoints.MaxAttackDamage);
        var sentMessages = (_connection as MockedGameConnection).SentMessages;

        sentMessages.Should().ContainEquivalentOf(new ChatOutcoming {Message = $"Weapon Damage: 10-16"}, Config);
        sentMessages.Should()
            .ContainEquivalentOf(new ChatOutcoming {Message = $"Attack Damage: {minAttack}-{maxAttack}"}, Config);

        EquivalencyAssertionOptions<ChatOutcoming> Config(EquivalencyAssertionOptions<ChatOutcoming> cfg) =>
            cfg.Including(x => x.Message);
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
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);

        await _commandManager.Handle(_connection, $"/exp 500 \"{player2.Name}\"");

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
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);

        await _commandManager.Handle(_connection, $"/give \"{player2.Name}\" 1 10");

        player2.Inventory.Items.Should().NotBeEmpty();
        player2.Inventory.Items.Should().ContainEquivalentOf(new ItemInstance
        {
            ItemId = 1,
            Count = 10
        }, cfg => cfg.Including(x => x.ItemId).Including(x => x.Count));
    }

    [Fact]
    public async Task GiveItemCommand_InvalidPlayer()
    {
        await _commandManager.Handle(_connection, "/give missing 1 10");

        ((MockedGameConnection) _connection).SentMessages.Should().ContainEquivalentOf(new ChatOutcoming
        {
            Message = "Target not found"
        }, cfg => cfg.Including(x => x.Message));
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
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);

        player2.GetPoint(EPoints.Gold).Should().Be(0);
        await _commandManager.Handle(_connection, $"/gold 10 \"{player2.Name}\"");
        player2.GetPoint(EPoints.Gold).Should().Be(10);
    }

    [Fact]
    public async Task GotoCommand_Coords()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);
        world.Update(0); // spawn entities

        _player.Move((int) (Map.MapUnit * 10), (int) (Map.MapUnit * 26));

        Assert.Equal((int) (10 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int) (26 * Map.MapUnit), _player.PositionY);

        await _commandManager.Handle(_connection, $"/goto {11} {27}");

        Assert.Equal((int) (_player.Map.PositionX + 11 * 100), _player.PositionX);
        Assert.Equal((int) (_player.Map.PositionY + 27 * 100), _player.PositionY);
    }

    [Fact]
    public async Task GotoCommand_Map()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);
        world.Update(0); // spawn entities


        Assert.Equal((int) (10 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int) (26 * Map.MapUnit), _player.PositionY);

        await _commandManager.Handle(_connection, "/goto --map map_b2");

        // target position is half of X & Y
        Assert.Equal((int) (13 * Map.MapUnit), _player.PositionX);
        Assert.Equal((int) (29 * Map.MapUnit), _player.PositionY);
    }

    [Fact]
    public async Task HelpCommand()
    {
        await _commandManager.Handle(_connection, "/help");

        (_connection as MockedGameConnection).SentMessages.Should().ContainEquivalentOf(new ChatOutcoming
            {
                Message = "The following commands are available:\n"
            }, cfg => cfg
                .Including(x => x.Message)
                .Using<string>(ctx => ctx.Subject.Should().StartWith(ctx.Expectation)).WhenTypeIs<string>()
        );
    }

    [Fact]
    public async Task KickCommand()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);

        await _commandManager.Handle(_connection, $"/kick \"{player2.Name}\"");

        Assert.Null(world.GetPlayer(player2.Name));
    }

    [Fact]
    public async Task KickCommand_Invalid()
    {
        await _commandManager.Handle(_connection, "/kick something");

        (_connection as MockedGameConnection).SentMessages.Should().ContainEquivalentOf(new ChatOutcoming
        {
            Message = "Target not found"
        }, cfg => cfg.Including(x => x.Message));
    }

    [Fact]
    public async Task LevelCommand_Self()
    {
        _player.GetPoint(EPoints.Level).Should().Be(1);

        await _commandManager.Handle(_connection, "/level 30");

        _player.GetPoint(EPoints.Level).Should().Be(30);
    }

    [Fact]
    public async Task LevelCommand_Other()
    {
        var world = await PrepareWorldAsync();
        var player2 = ActivatorUtilities.CreateInstance<PlayerEntity>(_services, _playerDataFaker.Generate());
        world.SpawnEntity(_player);
        world.SpawnEntity(player2);

        player2.GetPoint(EPoints.Level).Should().Be(1);

        await _commandManager.Handle(_connection, $"/level 30 \"{player2.Name}\"");

        player2.GetPoint(EPoints.Level).Should().Be(30);
    }

    [Fact]
    public async Task LogoutCommand()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);

        world.GetPlayer(_player.Name).Should().NotBeNull();

        await _commandManager.Handle(_connection, "/logout");

        world.GetPlayer(_player.Name).Should().BeNull();
    }

    [Fact]
    public async Task PhaseSelectCommand()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);

        world.GetPlayer(_player.Name).Should().NotBeNull();
        (_connection as MockedGameConnection).SentPhases.Should().NotContainEquivalentOf(new GCPhase
        {
            Phase = EPhases.Select
        });

        await _commandManager.Handle(_connection, "/phase_select");

        _player.Connection.Phase.Should().Be(EPhases.Select);
        (_connection as MockedGameConnection).SentPhases.Should().ContainEquivalentOf(new GCPhase
        {
            Phase = EPhases.Select
        });
        world.GetPlayer(_player.Name).Should().BeNull();
    }

    [Fact]
    public async Task QuitCommand()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);

        world.GetPlayer(_player.Name).Should().NotBeNull();

        await _commandManager.Handle(_connection, "/quit");

        world.GetPlayer(_player.Name).Should().BeNull();
    }

    [Fact]
    public void RestartHereCommand()
    {
        // TODO
    }

    [Fact]
    public void RestartTownCommand()
    {
        // TODO
    }

    [Fact]
    public async Task SpawnCommand_WithoutCount()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);
        world.Update(0); // spawn entities
        _player.Move((int) (Map.MapUnit * 13), (int) (Map.MapUnit * 29)); // center of the map
        await File.WriteAllTextAsync("settings.toml", @"maps = [""map_a2"", ""map_b2""]");
        _player.Map.Entities.Count.Should().Be(1);

        await _commandManager.Handle(_connection, "/spawn 101");
        world.Update(0); // spawn entities

        _player.Map.Entities.Count.Should().Be(2);
    }

    [Fact]
    public async Task SpawnCommand_WithCount()
    {
        var world = await PrepareWorldAsync();
        world.SpawnEntity(_player);
        world.Update(0); // spawn entities
        _player.Move((int) (Map.MapUnit * 13), (int) (Map.MapUnit * 29)); // center of the map
        await File.WriteAllTextAsync("settings.toml", @"maps = [""map_a2"", ""map_b2""]");
        _player.Map.Entities.Count.Should().Be(1);

        await _commandManager.Handle(_connection, "/spawn 101 10");
        world.Update(0); // spawn entities

        _player.Map.Entities.Count.Should().Be(11);
    }

    [Fact]
    public async Task StatCommand()
    {
        _player.AddPoint(EPoints.StatusPoints, 1);
        _player.GetPoint(EPoints.Ht).Should().Be(1);

        await _commandManager.Handle(_connection, "/stat ht");

        _player.GetPoint(EPoints.Ht).Should().Be(2);
    }

    private async Task<IWorld> PrepareWorldAsync()
    {
        if (!Directory.Exists("data")) Directory.CreateDirectory("data");
        await File.WriteAllTextAsync("data/atlasinfo.txt", $"map_a2	{Map.MapUnit * 10}	{Map.MapUnit * 26}	6	6\n" +
                                                           $"map_b2	{Map.MapUnit * 10}	{Map.MapUnit * 26}	6	6");
        var world = _services.GetRequiredService<IWorld>();
        await world.Load();
        return world;
    }

    [Fact]
    public async Task ReloadPermissionsCommand_WithoutTarget()
    {
        // Prepare
        var updatedGroup = Guid.NewGuid();
        var groupName = "test";

        var newPermissions = new[] {"reload_perms", "goto"};
        var cacheManager = _services.GetRequiredService<ICacheManager>();
        var commandRepo = _services.GetRequiredService<ICommandPermissionRepository>();
        commandRepo.GetPermissionsForGroupAsync(Arg.Any<Guid>()).Returns(newPermissions);
        commandRepo.GetGroupsAsync().Returns([
            new PermissionGroup
            {
                Id = updatedGroup,
                Name = groupName,
                Permissions = newPermissions
            }
        ]);

        cacheManager.CreateList<Guid>(Arg.Any<string>())
            .Range(0, 0).Returns(new[] {updatedGroup});

        // Act
        await _commandManager.Handle(_connection, "/reload_perms");

        // Assert
        _commandManager.Groups.Keys.Should().Contain(updatedGroup);
        _commandManager.Groups.Values.Should().ContainEquivalentOf(new PermissionGroup
        {
            Id = updatedGroup,
            Name = groupName,
            Permissions = newPermissions
        });

        await _commandManager.ReloadAsync();

        await _player.ReloadPermissions();

        ((MockedGameConnection) _connection).SentMessages.Should().ContainEquivalentOf(new ChatOutcoming
        {
            Message = "Permissions reloaded"
        }, cfg => cfg.Including(x => x.Message));
    }

    [Fact]
    public async Task InGameShopCommand()
    {
        // Prepare
        _services.GetRequiredService<IOptions<GameOptions>>().Value.InGameShop = "test";

        // Act
        await _commandManager.Handle(_connection, "/in_game_mall");

        // Assert
        ((MockedGameConnection) _connection).SentMessages.Should().ContainEquivalentOf(new ChatOutcoming
        {
            Message = "mall test",
            MessageType = ChatMessageTypes.Command
        }, cfg => cfg.Including(x => x.Message));
    }
}
