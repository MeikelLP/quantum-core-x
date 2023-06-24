using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;

namespace Game.Caching;

public class CachePlayerStore : ICachePlayerStore
{
    private readonly ICacheManager _cacheManager;
    public CachePlayerStore(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid playerId)
    {
        var playerKey = $"player:{playerId.ToString()}";
        if (await _cacheManager.Exists(playerKey) > 0)
        {
            return await _cacheManager.Get<PlayerData>(playerKey);
        }

        return null;
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid account, byte slot)
    {
        var key = $"players:{account.ToString()}";

        var list = _cacheManager.CreateList<Guid>(key);
        if (await _cacheManager.Exists(key) <= 0)
        {
            return null;
        }

        var playerId = await list.Index(slot);
        return await GetPlayerAsync(playerId);
    }

    public async Task SetPlayerAsync(PlayerData player)
    {
        var playerKey = $"player:{player.Id.ToString()}";
        await _cacheManager.Set(playerKey, player);
    }

    public async Task CreateAsync(PlayerData player)
    {
        // Add player to cache
        await _cacheManager.Set($"player:{player.Id.ToString()}", player);
        
        // Add player to the list of characters
        var list = _cacheManager.CreateList<Guid>($"players:{player.AccountId.ToString()}");
        await list.Push(player.Id);
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        // Delete player redis data
        var key = $"player:{player.Id.ToString()}";
        await _cacheManager.Del(key);

        key = $"players:{player.AccountId.ToString()}";
        var list = _cacheManager.CreateList<Guid>(key);
        await list.Rem(1, player.Id);

        // TODO delete items from players inventory
        key = $"items:{player.Id}:{(byte)WindowType.Inventory}";
        await _cacheManager.Del(key);
    }
}