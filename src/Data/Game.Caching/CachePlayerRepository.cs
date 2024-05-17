using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;

namespace Game.Caching;

public class CachePlayerRepository : ICachePlayerRepository
{
    private readonly ICacheManager _cacheManager;

    public CachePlayerRepository(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public async Task<PlayerData?> GetPlayerAsync(uint playerId)
    {
        var playerKey = $"player:{playerId.ToString()}";
        if (await _cacheManager.Exists(playerKey) > 0)
        {
            return await _cacheManager.Get<PlayerData>(playerKey);
        }

        return null;
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid accountId, byte slot)
    {
        var key = $"players:{accountId.ToString()}:{slot}";
        return await _cacheManager.Get<PlayerData>(key);
    }

    public async Task SetPlayerAsync(PlayerData player)
    {
        await _cacheManager.Set($"player:{player.Id.ToString()}", player);
        await _cacheManager.Set($"players:{player.AccountId.ToString()}:{player.Slot}", player);
    }

    public async Task CreateAsync(PlayerData player)
    {
        // Add player to cache
        await _cacheManager.Set($"player:{player.Id.ToString()}", player);

        var existingKeys = await _cacheManager.Keys($"players:{player.AccountId.ToString()}:*");
        var index = existingKeys.Length;
        await _cacheManager.Set($"players:{player.AccountId.ToString()}:{index}", player);
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        // Delete player redis data
        var key = $"player:{player.Id.ToString()}";
        await _cacheManager.Del(key);

        var keys = await _cacheManager.Keys($"players:{player.AccountId.ToString()}:*");
        foreach (var accountPlayerKey in keys)
        {
            var accountPlayer = await _cacheManager.Get<PlayerData>(accountPlayerKey);
            if (accountPlayer.Id == player.Id)
            {
                await _cacheManager.Del(accountPlayerKey);
                break;
            }
        }

        // TODO delete items from players inventory
        key = $"items:{player.Id}:{(byte) WindowType.Inventory}";
        await _cacheManager.Del(key);
    }

    public async Task<byte?> GetTempEmpireAsync(Guid accountId)
    {
        var empireRedisKey = $"temp:empire-selection:{accountId}";
        return await _cacheManager.Get<byte?>(empireRedisKey);
    }

    public async Task SetTempEmpireAsync(Guid accountId, byte empire)
    {
        await _cacheManager.Set($"temp:empire-selection:{accountId}", empire);
    }
}