using QuantumCore.Models;

namespace QuantumCore;

public interface IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(uint playerId);
}