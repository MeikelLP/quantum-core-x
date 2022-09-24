using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using Serilog;

namespace QuantumCore.Game.PacketHandlers.Select;

public class DeleteCharacterHandler : ISelectPacketHandler<DeleteCharacter>
{
    private readonly ILogger<DeleteCharacterHandler> _logger;
    private readonly IDatabaseManager _databaseManager;
    private readonly ICacheManager _cacheManager;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, IDatabaseManager databaseManager, ICacheManager cacheManager)
    {
        _logger = logger;
        _databaseManager = databaseManager;
        _cacheManager = cacheManager;
    }
    
    public async Task ExecuteAsync(PacketContext<DeleteCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Deleting character in slot {Slot}", ctx.Packet.Slot);

        if (ctx.Connection.AccountId == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character remove received before authorization");
            return;
        }

        var accountId = ctx.Connection.AccountId ?? default;

        var db = _databaseManager.GetAccountDatabase();
        var deletecode = await db.QueryFirstOrDefaultAsync<string>("SELECT DeleteCode FROM accounts WHERE Id = @Id", new { Id = ctx.Connection.AccountId });

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

        var player = await Player.GetPlayer(_databaseManager, _cacheManager, accountId, ctx.Packet.Slot);
        if (player == null)
        {
            ctx.Connection.Close();
            Log.Warning("Invalid or not exist character");
            return;
        }

        db = _databaseManager.GetGameDatabase();

        var delPlayer = new PlayerDeleted(player);
        await db.InsertAsync(delPlayer); // add the player to the players_deleted table

        await db.DeleteAsync(player); // delete the player from the players table

        // Delete player redis data
        var key = "player:" + player.Id;
        await _cacheManager.Del(key);

        key = "players:" + ctx.Connection.AccountId;
        var list = _cacheManager.CreateList<Guid>(key);
        await list.Rem(1, player.Id);

        // Delete items in redis cache

        //for (byte i = (byte)WindowType.Inventory; i < (byte) WindowType.Inventory; i++)
        {
            var items = _databaseManager.GetItems(_cacheManager, player.Id, (byte) WindowType.Inventory);

            await foreach (var item in items)
            {
                key = "item:" + item.Id;
                await _cacheManager.Del(key);
            }

            key = "items:" + player.Id + ":" + (byte) WindowType.Inventory;
            await _cacheManager.Del(key);
        }

        // Delete all items in db
        await db.QueryAsync("DELETE FROM items WHERE PlayerId=@PlayerId", new { PlayerId = player.Id });
    }
}