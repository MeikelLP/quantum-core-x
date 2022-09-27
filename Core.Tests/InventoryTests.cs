using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Weikio.PluginFramework.Catalogs;
using Xunit;

namespace Core.Tests;

public class InventoryTests
{
    private readonly ServiceProvider _services;
    private readonly IItemManager _itemManager;
    private readonly ICacheManager _cacheManager;

    public InventoryTests()
    {
        var itemManagerMock = new Mock<IItemManager>();
        var itemProtoData = new ItemData
        {
            Id = 1,
            Unknown = 0,
            Name = "Test Item",
            TranslatedName = "Test Item",
            Type = (byte)EItemType.Use,
            Subtype = 0,
            Unknown2 = 0,
            Size = 1,
            AntiFlags = 0,
            Flags = (uint)EItemFlags.ITEM_FLAG_STACKABLE,
            WearFlags = 0,
            ImmuneFlags = 0,
            BuyPrice = 0,
            SellPrice = 0,
            Limits = new List<ItemLimitData>(),
            Applies = new List<ItemApplyData>(),
            Values = new List<int>(),
            Sockets = new List<int>(),
            UpgradeId = 0,
            UpgradeSet = 0,
            MagicItemPercentage = 0,
            Specular = 0,
            SocketPercentage = 0
        };
        itemManagerMock.Setup(x => x.GetItem(It.IsAny<uint>())).Returns(itemProtoData);
        itemManagerMock.Setup(x =>
                x.CreateItem(It.IsAny<ItemData>(), It.IsAny<byte>()))
            .Returns<ItemData, byte>((protoData, count) => new ItemInstance
            {
                Count = count,
                Id = Guid.NewGuid(),
                ItemId = protoData.Id
            });
        _services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog())
            .Replace(new ServiceDescriptor(typeof(IItemManager), _ => itemManagerMock.Object, ServiceLifetime.Singleton))
            .AddSingleton(new Mock<IConnection>().Object)
            .AddSingleton<IPlayerEntity, PlayerEntity>()
            .AddSingleton<Player>()
            .AddSingleton<IWorld, World>()
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .BuildServiceProvider();
        World.Instance = _services.GetRequiredService<IWorld>();
        _itemManager = _services.GetRequiredService<IItemManager>();
        _cacheManager = _services.GetRequiredService<ICacheManager>();
    }

    [Fact]
    public async Task IncreaseItem_First()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 5);

        var inventoryItem = p.Inventory.GetItem(0);
        Assert.Equal((uint)1, inventoryItem.ItemId);
        Assert.Equal((uint)5, inventoryItem.Count);
    }

    [Fact]
    public async Task IncreaseItem_First_WithOverflow()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 200);
        await p.AddItem(1, 150);

        var inventoryItem1 = p.Inventory.GetItem(0);
        var inventoryItem2 = p.Inventory.GetItem(1);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)200, inventoryItem1.Count);
        Assert.Equal((uint)1, inventoryItem2.ItemId);
        Assert.Equal((uint)150, inventoryItem2.Count);
    }

    [Fact]
    public async Task IncreaseItem_Second()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, 50);
        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, 50);

        var inventoryItem1 = p.Inventory.GetItem(0);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)100, inventoryItem1.Count);
    }

    [Fact]
    public async Task IncreaseItem_Second_WithOverflow()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 50);
        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, 200);

        var inventoryItem1 = p.Inventory.GetItem(0);
        var inventoryItem2 = p.Inventory.GetItem(1);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)200, inventoryItem1.Count);
        Assert.Equal((uint)1, inventoryItem2.ItemId);
        Assert.Equal((uint)50, inventoryItem2.Count);
    }

    [Fact]
    public async Task IncreaseItem_FillNonEmptyStacks()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 50);
        await p.AddItem(1, 50);

        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, 350);

        var inventoryItem1 = p.Inventory.GetItem(0);
        var inventoryItem2 = p.Inventory.GetItem(1);
        var inventoryItem3 = p.Inventory.GetItem(2);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)200, inventoryItem1.Count);
        Assert.Equal((uint)1, inventoryItem2.ItemId);
        Assert.Equal((uint)200, inventoryItem2.Count);
        Assert.Equal((uint)1, inventoryItem3.ItemId);
        Assert.Equal((uint)50, inventoryItem3.Count);
    }

    [Fact]
    public async Task DecreaseItem()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 50);
        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, -20);

        var inventoryItem1 = p.Inventory.GetItem(0);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)30, inventoryItem1.Count);
    }

    [Fact]
    public async Task DecreaseItem_WithOverflow()
    {
        var p = _services.GetRequiredService<IPlayerEntity>();
        await p.AddItem(1, 200);
        await p.AddItem(1, 100);
        await p.AddItemAmountAsync(_itemManager, _cacheManager, 1, -150);

        var inventoryItem1 = p.Inventory.GetItem(0);
        Assert.Equal((uint)1, inventoryItem1.ItemId);
        Assert.Equal((uint)150, inventoryItem1.Count);
    }
}