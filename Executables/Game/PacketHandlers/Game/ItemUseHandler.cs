using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Constants;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemUseHandler : IGamePacketHandler<ItemUse>
{
    private readonly IItemManager _itemManager;
    private readonly IAffectManager _affectController;
    private readonly ILogger<ItemUseHandler> _logger;

    public ItemUseHandler(IItemManager itemManager, IAffectManager affectController, ILogger<ItemUseHandler> logger)
    {
        _itemManager = itemManager;
        _logger = logger;
        _affectController = affectController;
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
            await player.RemoveItem(item);
            if (await player.Inventory.PlaceItem(item))
            {
                await player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                await player.SendItem(item);
                await player.SendCharacterUpdate();
            }
            else
            {
                await player.SetItem(item, ctx.Packet.Window, ctx.Packet.Position);
                await player.SendChatInfo("Cannot unequip item if the inventory is full");
            }
        }
        else if (player.IsEquippable(item))
        {
            var wearSlot = player.Inventory.EquipmentWindow.GetWearSlot(_itemManager, item);

            if (wearSlot <= ushort.MaxValue)
            {
                var item2 = player.Inventory.EquipmentWindow.GetItem((ushort)wearSlot);

                if (item2 != null)
                {
                    await player.RemoveItem(item);
                    await player.RemoveItem(item2);
                    if (await player.Inventory.PlaceItem(item2))
                    {
                        await player.SendRemoveItem(ctx.Packet.Window, (ushort)wearSlot);
                        await player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                        await player.SetItem(item, ctx.Packet.Window, (ushort)wearSlot);
                        await player.SetItem(item2, ctx.Packet.Window, ctx.Packet.Position);
                        await player.SendItem(item);
                        await player.SendItem(item2);
                    }
                    else
                    {
                        await player.SetItem(item, ctx.Packet.Window, ctx.Packet.Position);
                        await player.SetItem(item2, ctx.Packet.Window, (ushort)wearSlot);
                        await player.SendChatInfo("Cannot swap item if the inventory is full");
                    }
                }
                else
                {
                    await player.RemoveItem(item);
                    await player.SetItem(item, (byte) WindowType.Inventory, (ushort)wearSlot);
                    await player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                    await player.SendItem(item);
                }
            }
        }else
        {
            switch ((EItemType) itemProto.Type)
            {
                case EItemType.Use:
                    _logger.LogDebug("Use item {ItemName}", itemProto.TranslatedName);
                    switch ((EUseSubTypes) itemProto.Subtype)
                    {
                        case EUseSubTypes.AbilityUp:
                            _logger.LogDebug("Use ability up");
                            var type = itemProto.Values[0];
                            var duration = itemProto.Values[1];
                            var value = itemProto.Values[2];
                            // TODO: Enums.NET improve allocations
                            var applyInfo = (EApplyType)type;
                            var applyType = AffectConstants.ApplyTypeToApplyPointMapping[applyInfo];
                            switch ((EApplyType) type)
                            {
                                case EApplyType.MovementSpeed:
                                    await _affectController.AddAffect(player, EAffectType.MovementSpeed, applyType, value, EAffects.MovSpeedPotion, duration, 0);
                                    break;
                                case EApplyType.AttackSpeed:
                                    await _affectController.AddAffect(player, EAffectType.AttackSpeed, applyType, value, EAffects.AttSpeedPotion, duration, 0);
                                    break;
                            }
                            break;
                        default:
                            _logger.LogWarning("Don't know how to handle item sub type {ItemSubType} for item {Id}", itemProto.Subtype, itemProto.Id);
                            break;
                    }
                    break;
            }
        }
    }
}
