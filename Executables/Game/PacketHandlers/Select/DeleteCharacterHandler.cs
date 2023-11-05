using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Data;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Select;

public class DeleteCharacterHandler : IGamePacketHandler<DeleteCharacter>
{
    private readonly ILogger<DeleteCharacterHandler> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly IItemRepository _itemRepository;
    private readonly IPlayerManager _playerManager;
    private readonly IAccountManager _accountManager;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, ICacheManager cacheManager,
        IItemRepository itemRepository, IPlayerManager playerManager, IAccountManager accountManager)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _itemRepository = itemRepository;
        _playerManager = playerManager;
        _accountManager = accountManager;
    }

    public async Task ExecuteAsync(GamePacketContext<DeleteCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Deleting character in slot {Slot}", ctx.Packet.Slot);

        if (ctx.Connection.AccountId == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character remove received before authorization");
            return;
        }

        var accountId = ctx.Connection.AccountId ?? default;

        var deletecode = await _accountManager.GetDeleteCodeAsync(accountId);

        if (deletecode == default)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalida ccount id??");
            return;
        }

        if (deletecode != ctx.Packet.Code[..^1])
        {
            ctx.Connection.Send(new DeleteCharacterFail());
            return;
        }

        ctx.Connection.Send(new DeleteCharacterSuccess
        {
            Slot = ctx.Packet.Slot
        });

        var player = await _playerManager.GetPlayer(accountId, ctx.Packet.Slot);
        if (player == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalid or not exist character");
            return;
        }

        await _playerManager.DeleteAsync(player);

        // Delete player redis data
        var key = "player:" + player.Id;
        await _cacheManager.Del(key);

        key = "players:" + ctx.Connection.AccountId;
        var list = _cacheManager.CreateList<Guid>(key);
        await list.Rem(1, player.Id);

        // Delete items in redis cache

        //for (byte i = (byte)WindowType.Inventory; i < (byte) WindowType.Inventory; i++)
        {
            var items = _itemRepository.GetItems(_cacheManager, player.Id, (byte) WindowType.Inventory);

            await foreach (var item in items)
            {
                key = "item:" + item.Id;
                await _cacheManager.Del(key);
            }

            key = "items:" + player.Id + ":" + (byte) WindowType.Inventory;
            await _cacheManager.Del(key);
        }

        // Delete all items in db
        await _itemRepository.DeletePlayerItemsAsync(player.Id);
    }
}
