using FluentAssertions;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;
using QuantumCore.Game.PlayerUtils;

namespace Game.Tests;

public class ItemProtoTests
{
    [Fact]
    public void CanRead()
    {
        var proto = ItemProto.FromFile("Fixtures/item_proto");
        var items = proto.Content.Data.Items;
        items.Should().HaveCount(1);
        // map to another type so we don't include any library properties
        new ItemData
        {
            Name = items[0].Name,
            AntiFlags = items[0].AntiFlags,
            Applies = items[0].Applies.Select(x => new ItemApplyData {Type = x.Type, Value = x.Value}).ToList(),
            Id = items[0].Id,
            Limits = items[0].Limits.Select(x => new ItemLimitData {Type = x.Type, Value = x.Value}).ToList(),
            Size = items[0].Size,
            Sockets = items[0].Sockets,
            Specular = items[0].Specular,
            Subtype = items[0].Subtype,
            Type = items[0].Type,
            Unknown = items[0].Unknown,
            Unknown2 = items[0].Unknown2,
            Values = items[0].Values,
            BuyPrice = items[0].BuyPrice,
            ImmuneFlags = items[0].ImmuneFlags,
            SellPrice = items[0].SellPrice,
            SocketPercentage = items[0].SocketPercentage,
            TranslatedName = items[0].TranslatedName,
            UpgradeId = items[0].UpgradeId,
            UpgradeSet = items[0].UpgradeSet,
            WearFlags = items[0].WearFlags,
            MagicItemPercentage = items[0].MagicItemPercentage
        }.Should().BeEquivalentTo(new ItemData
        {
            Name = "Item1",
            AntiFlags = (int) EAntiFlags.Shaman,
            Applies =
            [
                new ItemApplyData {Type = (byte) EApplyType.AttackSpeed, Value = 22},
                new ItemApplyData {Type = 0, Value = 0},
                new ItemApplyData {Type = 0, Value = 0},
            ],
            Id = 10,
            Limits =
            [
                new ItemLimitData {Type = (byte) ELimitType.Level, Value = 0},
                new ItemLimitData {Type = 0, Value = 0}
            ],
            Size = 2,
            Sockets = [0, 0, 0],
            Specular = 0,
            Subtype = (byte) EWeaponType.Sword,
            Type = (byte) EItemType.Weapon,
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
            WearFlags = (uint) EWearFlags.Weapon,
            MagicItemPercentage = 15
        });
    }
}
