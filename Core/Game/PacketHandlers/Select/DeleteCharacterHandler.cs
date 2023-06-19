using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Select;

public class DeleteCharacterHandler : IGamePacketHandler<DeleteCharacter>
{
    private readonly ILogger<DeleteCharacterHandler> _logger;
    private readonly IDbConnection _db;
    private readonly ICacheManager _cacheManager;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, IDbConnection db, ICacheManager cacheManager)
    {
        _logger = logger;
        _db = db;
        _cacheManager = cacheManager;
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

        var deletecode = await _db.QueryFirstOrDefaultAsync<string>("SELECT DeleteCode FROM accounts WHERE Id = @Id", new { Id = ctx.Connection.AccountId });

        if (deletecode == default)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalida ccount id??");
            return;
        }

        if (deletecode != ctx.Packet.Code[..^1])
        {
            await ctx.Connection.Send(new DeleteCharacterFail());
            return;
        }

        await ctx.Connection.Send(new DeleteCharacterSuccess
        {
            Slot = ctx.Packet.Slot
        });

        var player = await Player.GetPlayer(_db, _cacheManager, accountId, ctx.Packet.Slot);
        if (player == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalid or not exist character");
            return;
        }

        var delPlayer = new PlayerDeleted(player);
        await _db.InsertAsync(delPlayer); // add the player to the players_deleted table

        await _db.DeleteAsync(player); // delete the player from the players table

        // Delete player redis data
        var key = "player:" + player.Id;
        await _cacheManager.Del(key);

        key = "players:" + ctx.Connection.AccountId;
        var list = _cacheManager.CreateList<Guid>(key);
        await list.Rem(1, player.Id);

        // Delete items in redis cache

        //for (byte i = (byte)WindowType.Inventory; i < (byte) WindowType.Inventory; i++)
        {
            var items = _db.GetItems(_cacheManager, player.Id, (byte) WindowType.Inventory);

            await foreach (var item in items)
            {
                key = "item:" + item.Id;
                await _cacheManager.Del(key);
            }

            key = "items:" + player.Id + ":" + (byte) WindowType.Inventory;
            await _cacheManager.Del(key);
        }

        // Delete all items in db
        await _db.QueryAsync("DELETE FROM items WHERE PlayerId=@PlayerId", new { PlayerId = player.Id });
    }
}