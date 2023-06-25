using Game.Caching;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game;

public class PlayerManager : IPlayerManager
{
    private readonly IDbPlayerRepository _dbPlayerRepository;
    private readonly ICachePlayerRepository _cachePlayerRepository;
    private readonly ILogger<PlayerManager> _logger;
    private readonly IJobManager _jobManager;

    public PlayerManager(IDbPlayerRepository dbPlayerRepository, ICachePlayerRepository cachePlayerRepository, ILogger<PlayerManager> logger, IJobManager jobManager)
    {
        _dbPlayerRepository = dbPlayerRepository;
        _cachePlayerRepository = cachePlayerRepository;
        _logger = logger;
        _jobManager = jobManager;
    }

    public async Task<PlayerData?> GetPlayer(Guid account, byte slot)
    {
        return await _cachePlayerRepository.GetPlayerAsync(account, slot);
    }

    public async Task<PlayerData?> GetPlayer(Guid playerId)
    {
        var cachedPlayer = await _cachePlayerRepository.GetPlayerAsync(playerId);
        if (cachedPlayer is null)
        {
            var dbPlayer = await _dbPlayerRepository.GetPlayerAsync(playerId);
            if (dbPlayer is not null)
            {
                await _cachePlayerRepository.SetPlayerAsync(dbPlayer);
                return dbPlayer;
            }
            _logger.LogWarning("Could not find player with ID {PlayerId}", playerId);
        }
        return null;
    }

    public async Task<PlayerData[]> GetPlayers(Guid account)
    {
        // get players always from db not from cache

        var players = await _dbPlayerRepository.GetPlayersAsync(account);

        // update cache
        await Task.WhenAll(players.Select(x => _cachePlayerRepository.SetPlayerAsync(x)));

        return players;
    }

    public Task<bool> IsNameInUseAsync(string name)
    {
        return _dbPlayerRepository.IsNameInUseAsync(name);
    }

    public async Task<PlayerData> CreateAsync(Guid accountId, string playerName, byte @class, byte appearance)
    {
        var job = _jobManager.Get(@class);
        
        // Create player data
        var player = new PlayerData
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Name = playerName,
            PlayerClass = @class,
            PositionX = 958870,
            PositionY = 272788,
            St = job.St,
            Iq = job.Iq, 
            Dx = job.Dx, 
            Ht = job.Ht,
            Health = job.StartHp, 
            Mana = job.StartSp
        };
        
        var existingPlayers = await _dbPlayerRepository.GetPlayersAsync(player.AccountId);

        if (existingPlayers.Length >= PlayerConstants.MAX_PLAYERS_PER_ACCOUNT)
        {
            throw new InvalidOperationException("Already have max allowed players for this account");
        }
        
        await _dbPlayerRepository.CreateAsync(player);
        await _cachePlayerRepository.CreateAsync(player);

        // new index is equivalent to the previous length
        player.Slot = (byte)existingPlayers.Length;

        return player;
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _dbPlayerRepository.DeletePlayerAsync(player);
        await _cachePlayerRepository.DeletePlayerAsync(player);
    }
}

public static class PlayerConstants
{
    public const int MAX_PLAYERS_PER_ACCOUNT = 4;
}