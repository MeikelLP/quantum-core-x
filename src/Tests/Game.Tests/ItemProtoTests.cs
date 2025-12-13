using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;

namespace Game.Tests;

public class ItemProtoTests
{
    private readonly IItemManager _itemManager;

    public ItemProtoTests()
    {
        _itemManager = new ServiceCollection()
            .AddSingleton<IItemManager, ItemManager>()
            .AddLogging()
            .AddSingleton(_ =>
            {
                var mock = Substitute.For<IFileProvider>();
                mock.GetFileInfo(Arg.Any<string>()).ReturnsForAnyArgs(call =>
                    new PhysicalFileInfo(new FileInfo(Path.Combine("Fixtures", call.Arg<string>()))));
                return mock;
            })
            .BuildServiceProvider()
            .GetRequiredService<IItemManager>();
    }

    [Fact]
    public async Task CanRead()
    {
        await _itemManager.LoadAsync();
        var item = _itemManager.GetItem(10);
        // map to another type so we don't include any library properties
        item.Should().BeEquivalentTo(new ItemData
        {
            Name = "Item1",
            AntiFlags = (int)EAntiFlags.Shaman,
            Flags = 1u,
            Applies =
            [
                new ItemApplyData { Type = (byte)EApplyType.AttackSpeed, Value = 22 },
                new ItemApplyData { Type = 0, Value = 0 },
                new ItemApplyData { Type = 0, Value = 0 },
            ],
            Id = 10,
            Limits =
            [
                new ItemLimitData { Type = (byte)ELimitType.Level, Value = 0 },
                new ItemLimitData { Type = 0, Value = 0 }
            ],
            Size = 2,
            Sockets = [0, 0, 0],
            Specular = 0,
            Subtype = (byte)EWeaponType.Sword,
            Type = (byte)EItemType.Weapon,
            Unknown = 0,
            Unknown2 = 0,
            Values = [0, 15, 19, 13, 15, 0],
            BuyPrice = 0,
            ImmuneFlags = 0,
            SellPrice = 0,
            SocketPercentage = 1,
            TranslatedName = "Sword+0",
            UpgradeId = 11,
            UpgradeSet = 1,
            WearFlags = (uint)EWearFlags.Weapon,
            MagicItemPercentage = 15
        });
    }

    [Fact]
    public async Task CanGetApplyValue()
    {
        await _itemManager.LoadAsync();
        var item = _itemManager.GetItem(10);
        var value = item!.GetApplyValue(EApplyType.AttackSpeed);
        value.Should().Be(22);
    }
}
