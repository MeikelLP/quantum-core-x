using Game.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game;

public class PlayerManager : IPlayerManager
{
    private readonly IDbPlayerRepository _dbPlayerRepository;
    private readonly ICachePlayerRepository _cachePlayerRepository;
    private readonly ILogger<PlayerManager> _logger;
    private readonly IJobManager _jobManager;
    private readonly GameOptions _gameOptions;

    public PlayerManager(IDbPlayerRepository dbPlayerRepository, ICachePlayerRepository cachePlayerRepository,
        ILogger<PlayerManager> logger, IJobManager jobManager, IOptions<GameOptions> gameOptions)
    {
        _dbPlayerRepository = dbPlayerRepository;
        _cachePlayerRepository = cachePlayerRepository;
        _logger = logger;
        _jobManager = jobManager;
        _gameOptions = gameOptions.Value;
    }

    public async Task<PlayerData?> GetPlayer(Guid accountId, byte slot)
    {
        var cachedPlayer = await _cachePlayerRepository.GetPlayerAsync(accountId, slot);
        if (cachedPlayer is null)
        {
            var players = await _dbPlayerRepository.GetPlayersAsync(accountId);
            for (var i = 0; i < players.Length; i++)
            {
                var player = players[i];
                player.Slot = (byte)i;

                if (i == slot)
                {
                    await _cachePlayerRepository.SetPlayerAsync(player);
                    return player;
                }
            }

            _logger.LogWarning("Could not find player for account {AccountId} at slot {Slot}", accountId, slot);
        }

        return cachedPlayer;
    }

    public async Task<PlayerData?> GetPlayer(uint playerId)
    {
        var cachedPlayer = await _cachePlayerRepository.GetPlayerAsync(playerId);
        if (cachedPlayer is null)
        {
            var player = await _dbPlayerRepository.GetPlayerAsync(playerId);
            if (player == null)
            {
                _logger.LogWarning("Could not find player with ID {PlayerId}", playerId);
            }
            else
            {
                await _cachePlayerRepository.SetPlayerAsync(player);
                return player;
            }
        }

        return cachedPlayer;
    }

    public async Task SetPlayerAsync(PlayerData data)
    {
        await _cachePlayerRepository.SetPlayerAsync(data);
        await _dbPlayerRepository.SetPlayerAsync(data);
    }

    public async Task<PlayerData[]> GetPlayers(Guid accountId)
    {
        // get players always from db not from cache

        var players = await _dbPlayerRepository.GetPlayersAsync(accountId);

        // update cache
        await Task.WhenAll(players.Select((x, i) => _cachePlayerRepository.SetPlayerAsync(x)));

        return players;
    }

    public Task<bool> IsNameInUseAsync(string name)
    {
        return _dbPlayerRepository.IsNameInUseAsync(name);
    }

    public async Task<PlayerData> CreateAsync(Guid accountId, string playerName, EPlayerClassGendered @class,
        byte appearance)
    {
        var job = _jobManager.Get(@class);

        var existingPlayers = await _dbPlayerRepository.GetPlayersAsync(accountId);

        if (existingPlayers.Length >= PlayerConstants.MAX_PLAYERS_PER_ACCOUNT)
        {
            throw new InvalidOperationException("Already have max allowed players for this account");
        }

        byte empire;
        if (existingPlayers.Length > 0)
        {
            // reuse empire from first character
            empire = existingPlayers[0].Empire;
        }
        else
        {
            var empireFromCache = await _cachePlayerRepository.GetTempEmpireAsync(accountId);
            if (empireFromCache is null)
            {
                _logger.LogError("No empire has been selected before. This should not happen");
                throw new InvalidOperationException("No empire has been selected before. This should not happen.");
            }

            empire = empireFromCache.Value;
        }

        // Create player data
        var player = new PlayerData
        {
            Id = 0,
            AccountId = accountId,
            Name = playerName,
            PlayerClass = @class,
            PositionX = _gameOptions.Empire[empire].X,
            PositionY = _gameOptions.Empire[empire].Y,
            St = job.St,
            Iq = job.Iq,
            Dx = job.Dx,
            Ht = job.Ht,
            Health = job.StartHp,
            Mana = job.StartSp,
            Empire = empire,
            Slot = (byte)existingPlayers.Length
        };


        await _dbPlayerRepository.CreateAsync(player);
        await _cachePlayerRepository.CreateAsync(player);

        return player;
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _dbPlayerRepository.DeletePlayerAsync(player);
        await _cachePlayerRepository.DeletePlayerAsync(player);
    }

    public async Task SetPlayerEmpireAsync(Guid accountId, uint playerId, byte empire)
    {
        await _dbPlayerRepository.UpdateEmpireAsync(accountId, playerId, empire);
    }
}
