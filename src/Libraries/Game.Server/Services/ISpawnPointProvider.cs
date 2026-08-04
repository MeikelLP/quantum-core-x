using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

public interface ISpawnPointProvider
{
    Task<SpawnPoint[]> GetSpawnPointsForMapAsync(string name);
}