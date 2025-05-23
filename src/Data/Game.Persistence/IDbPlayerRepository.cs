﻿using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerRepository : IPlayerRepository
{
    Task<PlayerData[]> GetPlayersAsync(Guid accountId);
    Task<bool> IsNameInUseAsync(string name);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);
    Task UpdateEmpireAsync(Guid accountId, uint playerId, EEmpire empire);
    Task SetPlayerAsync(PlayerData data);
}
