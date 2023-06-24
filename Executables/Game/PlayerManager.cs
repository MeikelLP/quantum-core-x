using Game.Caching;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game;

public class PlayerManager : IPlayerManager
{
    private readonly IDbPlayerStore _dbPlayerStore;
    private readonly ICachePlayerStore _cachePlayerStore;
    private readonly ILogger<PlayerManager> _logger;

    public PlayerManager(IDbPlayerStore dbPlayerStore, ICachePlayerStore cachePlayerStore, ILogger<PlayerManager> logger)
    {
        _dbPlayerStore = dbPlayerStore;
        _cachePlayerStore = cachePlayerStore;
        _logger = logger;
    }

    public async Task<PlayerData?> GetPlayer(Guid account, byte slot)
    {
        return await _cachePlayerStore.GetPlayerAsync(account, slot);
    }

    public async Task<PlayerData?> GetPlayer(Guid playerId)
    {
        var cachedPlayer = await _cachePlayerStore.GetPlayerAsync(playerId);
        if (cachedPlayer is null)
        {
            var dbPlayer = await _dbPlayerStore.GetPlayerAsync(playerId);
            if (dbPlayer is not null)
            {
                await _cachePlayerStore.SetPlayerAsync(dbPlayer);
                return dbPlayer;
            }
            _logger.LogWarning("Could not find player with ID {PlayerId}", playerId);
        }
        return null;
    }

    public async Task<PlayerData[]> GetPlayers(Guid account)
    {
        // get players always from db not from cache

        var players = await _dbPlayerStore.GetPlayersAsync(account);

        // update cache
        await Task.WhenAll(players.Select(x => _cachePlayerStore.SetPlayerAsync(x)));

        return players;
    }

    public Task<bool> IsNameInUseAsync(string name)
    {
        return _dbPlayerStore.IsNameInUseAsync(name);
    }

    public async Task<byte> CreateAsync(PlayerData player)
    {
        var existingPlayers = await _dbPlayerStore.GetPlayersAsync(player.AccountId);

        if (existingPlayers.Length >= PlayerConstants.MAX_PLAYERS_PER_ACCOUNT)
        {
            throw new InvalidOperationException("Already have max allowed players for this account");
        }
        
        await _dbPlayerStore.CreateAsync(player);
        await _cachePlayerStore.CreateAsync(player);

        // new index is equivalent to the previous length
        return (byte)existingPlayers.Length;
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _dbPlayerStore.DeletePlayerAsync(player);
        await _cachePlayerStore.DeletePlayerAsync(player);
    }
}

public static class PlayerConstants
{
    public const int MAX_PLAYERS_PER_ACCOUNT = 4;
}