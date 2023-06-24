using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.Caching;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game;

public class PlayerFactory : IPlayerFactory
{
    private readonly IDbConnection _db;
    private readonly ICacheManager _cacheManager;

    public PlayerFactory(IDbConnection db, ICacheManager cacheManager)
    {
        _db = db;
        _cacheManager = cacheManager;
    }

    public async Task<Player?> GetPlayer(Guid account, byte slot)
    {
        var key = "players:" + account;

        var list = _cacheManager.CreateList<Guid>(key);
        if (await _cacheManager.Exists(key) <= 0)
        {
            var i = 0;
            await foreach (var player in GetPlayers(account))
            {
                if (i == slot) return player;
                i++;
            }

            return null;
        }

        var playerId = await list.Index(slot);
        return await GetPlayer(playerId);
    }

    public async Task<Player?> GetPlayer(Guid playerId)
    {
        var playerKey = "player:" + playerId;
        if (await _cacheManager.Exists(playerKey) > 0)
        {
            return await _cacheManager.Get<Player>(playerKey);
        }
        else
        {
            var player = _db.Get<Player>(playerId);
            //var player = await SqlMapperExtensions.Get<Player>(db, playerId);
            await _cacheManager.Set(playerKey, player);
            return player;
        }
    }

    public async IAsyncEnumerable<Player> GetPlayers(Guid account)
    {
        var key = "players:" + account;

        var list = _cacheManager.CreateList<Guid>(key);

        // Check if we have players cached
        if (await _cacheManager.Exists(key) > 0)
        {
            // We have the characters cached
            var cachedIds = await list.Range(0, -1);

            foreach (var id in cachedIds)
            {
                yield return await _cacheManager.Get<Player>("player:" + id);
            }
        }
        else
        {
            var ids = await _db.QueryAsync("SELECT Id FROM players WHERE AccountId = @AccountId",
                new { AccountId = account });

            // todo: is it ever possible that we have a player cached but not the players list? 
            //  if this is not the case we can make this part short and faster
            foreach (var row in ids)
            {
                Guid playerId = row.Id;
                await list.Push(playerId);

                yield return await GetPlayer(playerId);
            }
        }
    }
}