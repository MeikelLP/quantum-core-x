using QuantumCore.Game.Persistence;

namespace QuantumCore.Game;

public interface IPlayerFactory
{
    Task<Player?> GetPlayer(Guid account, byte slot);
    Task<Player?> GetPlayer(Guid playerId);
    IAsyncEnumerable<Player> GetPlayers(Guid account);
}