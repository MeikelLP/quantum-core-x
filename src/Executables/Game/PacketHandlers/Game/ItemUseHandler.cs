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

        switch ((EItemType)itemProto.Type)
        {
            case EItemType.Armor:
            case EItemType.Weapon:
            case EItemType.Rod:
            case EItemType.Pick:
                {
                    if (ctx.Packet.Window == (byte)WindowType.Inventory && ctx.Packet.Position >= player.Inventory.Size)
                    {
                        await player.UnequipItem(item, ctx.Packet.Window, ctx.Packet.Position);
                    }

                    else if (player.IsEquippable(item))
                    {
                        await player.EquipItem(item, ctx.Packet.Window, ctx.Packet.Position);
                    }

                    break;
                }

            default:
                {
                    player.SendChatInfo($"Unknown item type {itemProto.Type}");
                    break;
                }
        }


    }
}
