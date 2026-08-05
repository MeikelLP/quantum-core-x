using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Items;

namespace Game.Caching;

public class CachePlayerRepository : ICachePlayerRepository
{
    private readonly IRedisStore _cacheManager;

    public CachePlayerRepository(ICacheManager cacheManager)
    {
        ArgumentNullException.ThrowIfNull(cacheManager);
        _cacheManager = cacheManager.Server;
    }

    public async Task<PlayerData?> GetPlayerAsync(uint playerId)
    {
        var playerKey = $"player:{playerId.ToString()}";
        if (await _cacheManager.ExistsAsync(playerKey) > 0)
        {
            return await _cacheManager.GetAsync<PlayerData>(playerKey);
        }

        return null;
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid accountId, byte slot)
    {
        var key = $"players:{accountId.ToString()}:{slot}";
        return await _cacheManager.GetAsync<PlayerData>(key);
    }

    public async Task SetPlayerAsync(PlayerData player)
    {
        ArgumentNullException.ThrowIfNull(player);
        await _cacheManager.SetAsync($"player:{player.Id.ToString()}", player);
        await _cacheManager.SetAsync($"players:{player.AccountId.ToString()}:{player.Slot}", player);
    }

    public async Task CreateAsync(PlayerData player)
    {
        ArgumentNullException.ThrowIfNull(player);
        // Add player to cache
        await _cacheManager.SetAsync($"player:{player.Id.ToString()}", player);

        var existingKeys = await _cacheManager.KeysAsync($"players:{player.AccountId.ToString()}:*");
        var index = existingKeys.Length;
        await _cacheManager.SetAsync($"players:{player.AccountId.ToString()}:{index}", player);
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        ArgumentNullException.ThrowIfNull(player);
        // Delete player redis data
        var key = $"player:{player.Id.ToString()}";
        await _cacheManager.DelAsync(key);

        var keys = await _cacheManager.KeysAsync($"players:{player.AccountId.ToString()}:*");
        foreach (var accountPlayerKey in keys)
        {
            var accountPlayer = await _cacheManager.GetAsync<PlayerData>(accountPlayerKey);
            if (accountPlayer.Id == player.Id)
            {
                await _cacheManager.DelAsync(accountPlayerKey);
                break;
            }
        }

        // TODO delete items from players inventory
        key = $"items:{player.Id}:{(byte)WindowType.INVENTORY}";
        await _cacheManager.DelAsync(key);
    }

    public async Task<EEmpire?> GetTempEmpireAsync(Guid accountId)
    {
        var empireRedisKey = $"temp:empire-selection:{accountId}";
        return await _cacheManager.GetAsync<EEmpire?>(empireRedisKey);
    }

    public async Task SetTempEmpireAsync(Guid accountId, EEmpire empire)
    {
        await _cacheManager.SetAsync($"temp:empire-selection:{accountId}", empire);
    }
}