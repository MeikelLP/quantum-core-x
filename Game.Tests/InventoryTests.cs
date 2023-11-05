using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;
using QuantumCore.Core.Cache;
using QuantumCore.Game.PlayerUtils;

namespace Game.Tests;

public class InventoryTests
{
    [Fact]
    public void SetEquipment_TriggersWearEvent()
    {
        var itemManager = Substitute.For<IItemManager>();
        itemManager
            .GetItem(Arg.Any<uint>())
            .Returns(new ItemData
            {
                WearFlags = (uint)EWearFlags.Body,
                Size = 1
            });
        var inv = new Inventory(itemManager,
            Substitute.For<ICacheManager>(), Substitute.For<ILogger>(), Substitute.For<IItemRepository>(), Guid.Empty, 1, 1, 1, 1);
        var changed = 0;
        inv.OnSlotChanged += (_, _) => changed++;

        var pos = (ushort)inv.EquipmentWindow.GetWearPosition(itemManager, 1);
        inv.SetEquipment(new ItemInstance { ItemId = 1 }, pos);

        inv.EquipmentWindow.Body!.ItemId.Should().Be(1);
        changed.Should().Be(1);
    }
}
