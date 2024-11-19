using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

public interface ISpawnPointProvider
{
    Task<SpawnPoint[]> GetSpawnPointsForMap(string name);
}