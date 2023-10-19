using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemUseHandler : IGamePacketHandler<ItemUse>
{
    private readonly IItemManager _itemManager;
    private readonly ILogger<ItemUseHandler> _logger;

    public ItemUseHandler(IItemManager itemManager, ILogger<ItemUseHandler> logger)
    {
        _itemManager = itemManager;
        _logger = logger;
    }

    public async Task ExecuteAsync(GamePacketContext<ItemUse> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        _logger.LogDebug("Use item {Window},{Position}", ctx.Packet.Window, ctx.Packet.Position);

        var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
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

        if (ctx.Packet.Window == (byte) WindowType.Inventory && ctx.Packet.Position >= player.Inventory.Size)
        {
            player.RemoveItem(item);
            if (await player.Inventory.PlaceItem(item))
            {
                player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                player.SendItem(item);
                player.SendCharacterUpdate();
            }
            else
            {
                player.SetItem(item, ctx.Packet.Window, ctx.Packet.Position);
                player.SendChatInfo("Cannot unequip item if the inventory is full");
            }
        }
        else if (player.IsEquippable(item))
        {
            var wearSlot = player.Inventory.EquipmentWindow.GetWearPosition(_itemManager, item);

            if (wearSlot <= ushort.MaxValue)
            {
                var item2 = player.Inventory.EquipmentWindow.GetItem((ushort)wearSlot);

                if (item2 != null)
                {
                    player.RemoveItem(item);
                    player.RemoveItem(item2);
                    if (await player.Inventory.PlaceItem(item2))
                    {
                        player.SendRemoveItem(ctx.Packet.Window, (ushort)wearSlot);
                        player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                        player.SetItem(item, ctx.Packet.Window, (ushort)wearSlot);
                        player.SetItem(item2, ctx.Packet.Window, ctx.Packet.Position);
                        player.SendItem(item);
                        player.SendItem(item2);
                    }
                    else
                    {
                        player.SetItem(item, ctx.Packet.Window, ctx.Packet.Position);
                        player.SetItem(item2, ctx.Packet.Window, (ushort)wearSlot);
                        player.SendChatInfo("Cannot swap item if the inventory is full");
                    }
                }
                else
                {
                    player.RemoveItem(item);
                    player.SetItem(item, (byte) WindowType.Inventory, (ushort)wearSlot);
                    player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                    player.SendItem(item);
                }
            }
        }
    }
}
