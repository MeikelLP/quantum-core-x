using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemUseHandler : IGamePacketHandler<ItemUse>
{
    private readonly IItemManager _itemManager;
    private readonly ILogger<ItemUseHandler> _logger;
    private readonly SkillsOptions _skillsOptions;
    private readonly IDropProvider _dropProvider;
    private readonly ICacheManager _cacheManager;

    public ItemUseHandler(IItemManager itemManager, ILogger<ItemUseHandler> logger, IOptions<GameOptions> gameOptions, IDropProvider dropProvider, ICacheManager cacheManager)
    {
        _itemManager = itemManager;
        _logger = logger;
        _skillsOptions = gameOptions.Value.Skills;
        _dropProvider = dropProvider;
        _cacheManager = cacheManager;
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
            case EItemType.Costume:
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
            case EItemType.Giftbox:
                {
                    var drop = _dropProvider.CalculateSpecialItemDrop(itemProto.Id);

                    if (drop == null)
                    {
                        player.SendChatInfo("Cannot find drops for item.");
                        return;
                    }

                    if (!await player.Inventory.PlaceItem(drop))
                    {
                        player.SendChatInfo("No place in inventory");
                        return;
                    }

                    await drop.Persist(_cacheManager);
                    player.SendItem(drop);

                    byte newCount = (byte)(item.Count - 1);

                    await player.ChangeItemQuantity(item, newCount);

                    break;
                }
            case EItemType.Skillbook:
                {
                    var skillId = itemProto.Id == _skillsOptions.GenericSkillBookId
                        ? itemProto.Sockets[0]
                        : itemProto.Values[0];

                    if (!Enum.TryParse<ESkillIndexes>(skillId.ToString(), out var skill))
                    {
                        _logger.LogWarning("Skill with Id({SkillId}) not defined", skillId);
                        return;
                    }

                    if (!player.Skills.LearnSkillByBook(skill)) return;

                    var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var delay = CoreRandom.GenerateInt32(_skillsOptions.SkillBookDelayMin, _skillsOptions.SkillBookDelayMax + 1);

                    player.Skills.SetSkillNextReadTime(skill, (int)currentTime + delay);
                    player.RemoveItem(item);
                    player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
                    break;
                }
            default:
                {
                    player.SendChatInfo($"Unknown item type {itemProto.Type}");
                    break;
                }
        }

        // TODO : what type of item is SS?
        if (itemProto.Id == _skillsOptions.SoulStoneId)
        {

            player.RemoveItem(item);
            player.SendRemoveItem(ctx.Packet.Window, ctx.Packet.Position);
        }

    }
}
