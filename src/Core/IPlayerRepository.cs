using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(uint playerId);
}