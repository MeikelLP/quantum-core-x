using System;
using System.Data;
using System.Threading.Tasks;
using AutoBogus;
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
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
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

    public CommandTests(ITestOutputHelper testOutputHelper)
    {
        var itemManagerMock = new Mock<IItemManager>();
        itemManagerMock.Setup(x => x.GetItem(It.IsAny<uint>())).Returns(() => new AutoFaker<ItemData>().Generate());
        var connectionMock = new Mock<IGameConnection>();
        var cacheManagerMock = new Mock<ICacheManager>();
        var redisListWrapperMock = new Mock<IRedisListWrapper<Guid>>();
        redisListWrapperMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new[] { QuantumCore.Game.Commands.CommandManager.Operator_Group });
        cacheManagerMock.Setup(x => x.CreateList<Guid>(It.IsAny<string>()))
            .Returns(redisListWrapperMock.Object);
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
            .AddSingleton(_ => new AutoFaker<Player>().Generate())
            .BuildServiceProvider();
        _commandManager = _services.GetRequiredService<ICommandManager>();
        _commandManager.Register("QuantumCore.Game.Commands", typeof(SpawnCommand).Assembly);
        _connection = _services.GetRequiredService<IGameConnection>();
        connectionMock.Setup(x => x.Player).Returns(_services.GetRequiredService<IPlayerEntity>()); // this would usually happen during char select
    }

    [Fact]
    public async Task ClearInventoryCommand()
    {
        var player = _services.GetRequiredService<IPlayerEntity>();
        await player.Load();
        await player.Inventory.PlaceItem(new ItemInstance
        {
            Id = Guid.NewGuid(),
            Count = 1,
            ItemId = 1
        });

        Assert.NotEmpty(player.Inventory.Items);
        await _commandManager.Handle(_connection, "/ip");
        
        Assert.Empty(player.Inventory.Items);
    }

    [Fact]
    public async Task CommandCache()
    {
    }

    [Fact]
    public async Task CommandManager()
    {
    }

    [Fact]
    public async Task CommandTeleport()
    {
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
}