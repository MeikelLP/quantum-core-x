using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;
using QuantumCore.Core.Cache;

namespace QuantumCore;

public class PlayerManager : IPlayerManager
{
    private readonly IPlayerRepository _repository;
    private readonly ICacheManager _cacheManager;

    public PlayerManager(IPlayerRepository repository, ICacheManager cacheManager)
    {
        _repository = repository;
        _cacheManager = cacheManager;
    }

    public async Task<PlayerData?> GetPlayer(Guid account, byte slot)
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

    public async Task<PlayerData> GetPlayer(Guid playerId)
    {
        var playerKey = "player:" + playerId;
        if (await _cacheManager.Exists(playerKey) > 0)
        {
            return await _cacheManager.Get<PlayerData>(playerKey);
        }
        else
        {
            var player = await _repository.GetPlayerAsync(playerId);
            //var player = await SqlMapperExtensions.Get<Player>(db, playerId);
            await _cacheManager.Set(playerKey, player);
            return player;
        }
    }

    public async IAsyncEnumerable<PlayerData> GetPlayers(Guid account)
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
                yield return await _cacheManager.Get<PlayerData>("player:" + id);
            }
        }
        else
        {
            var ids = await _repository.GetPlayerIdsForAccountAsync(account);

            // todo: is it ever possible that we have a player cached but not the players list?
            //  if this is not the case we can make this part short and faster
            foreach (var playerId in ids)
            {
                await list.Push(playerId);

                yield return await GetPlayer(playerId);
            }
        }
    }

    public Task DeleteAsync(PlayerData player)
    {
        // TODO cleanup adjust cache
        return _repository.DeleteAsync(player);
    }
}
