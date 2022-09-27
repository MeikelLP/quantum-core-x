using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Cache;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemUseHandler : IPacketHandler<ItemUse>
{
    private readonly IItemManager _itemManager;
    private readonly ILogger<ItemUseHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public ItemUseHandler(IItemManager itemManager, ILogger<ItemUseHandler> logger, ICacheManager cacheManager)
    {
        _itemManager = itemManager;
        _logger = logger;
        _cacheManager = cacheManager;
    }
        
    public async Task ExecuteAsync(PacketContext<ItemUse> ctx, CancellationToken token = default)
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

        if ((EItemType) itemProto.Type == EItemType.Use)
        {
            if ((EUseItemSubType) itemProto.Subtype == EUseItemSubType.USE_ABILITY_UP)
            {
                if (itemProto.TryGetItemUseApplyType(out var applyType) && player.Player.Effects.All(x => x.Type != applyType))
                {
                    var duration = (uint)itemProto.Values[1];
                    var value = (short)itemProto.Values[2];
                    switch (applyType)
                    {
                        case EEffectType.POINT_MOV_SPEED:
                            await player.TryAddEffect(new EffectData(EEffectType.POINT_MOV_SPEED, EAffectFlags.MOV_SPEED_POTION, duration, value));
                            await player.AddPoint(EPoints.MoveSpeed, value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    await player.AddItemAmountAsync(_itemManager, _cacheManager, itemProto.Id, -1);
                    await player.SendInventory();
                    await player.SendPoints();
                    await player.SendCharacterUpdate();
                }
            }
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
        }
    }
}