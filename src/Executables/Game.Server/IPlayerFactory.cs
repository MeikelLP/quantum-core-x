using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game;

public interface IPlayerFactory
{
    /// <summary>
    /// Creates a player entity and triggers the loads additional data before returning the entity itself.
    /// </summary>
    Task<IPlayerEntity> CreatePlayerAsync(IGameConnection connection, PlayerData player);
}