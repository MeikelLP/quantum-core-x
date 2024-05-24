using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ItemUse))]
public class ItemUseHandler
{
    private readonly IItemManager _itemManager;
    private readonly ILogger<ItemUseHandler> _logger;

    public ItemUseHandler(IItemManager itemManager, ILogger<ItemUseHandler> logger)
    {
        _itemManager = itemManager;
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, ItemUse packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        _logger.LogDebug("Use item {Window},{Position}", packet.Window, packet.Position);

        var item = player.GetItem(packet.Window, packet.Position);
        if (item == null)
        {
            _logger.LogDebug("Used item not found!");
            return;
        }

        var itemProto = _itemManager.GetItem(item.ItemId);
        if (itemProto == null)
        {
            _logger.LogDebug("Cannot find item proto {ItemId}", item.ItemId);
            return;
        }

        if (packet.Window == (byte) WindowType.Inventory && packet.Position >= player.Inventory.Size)
        {
            player.RemoveItem(item);
            if (await player.Inventory.PlaceItem(item))
            {
                player.SendRemoveItem(packet.Window, packet.Position);
                player.SendItem(item);
                player.SendCharacterUpdate();
            }
            else
            {
                player.SetItem(item, packet.Window, packet.Position);
                player.SendChatInfo("Cannot unequip item if the inventory is full");
            }
        }
        else if (player.IsEquippable(item))
        {
            var wearSlot = player.Inventory.EquipmentWindow.GetWearPosition(_itemManager, item.ItemId);

            if (wearSlot <= ushort.MaxValue)
            {
                var item2 = player.Inventory.EquipmentWindow.GetItem((ushort) wearSlot);

                if (item2 != null)
                {
                    player.RemoveItem(item);
                    player.RemoveItem(item2);
                    if (await player.Inventory.PlaceItem(item2))
                    {
                        player.SendRemoveItem(packet.Window, (ushort) wearSlot);
                        player.SendRemoveItem(packet.Window, packet.Position);
                        player.SetItem(item, packet.Window, (ushort) wearSlot);
                        player.SetItem(item2, packet.Window, packet.Position);
                        player.SendItem(item);
                        player.SendItem(item2);
                    }
                    else
                    {
                        player.SetItem(item, packet.Window, packet.Position);
                        player.SetItem(item2, packet.Window, (ushort) wearSlot);
                        player.SendChatInfo("Cannot swap item if the inventory is full");
                    }
                }
                else
                {
                    player.RemoveItem(item);
                    player.SetItem(item, (byte) WindowType.Inventory, (ushort) wearSlot);
                    player.SendRemoveItem(packet.Window, packet.Position);
                    player.SendItem(item);
                }
            }
        }
    }
}