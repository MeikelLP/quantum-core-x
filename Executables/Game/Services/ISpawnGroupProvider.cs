using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Services;

public interface ISpawnGroupProvider
{
    Task<IEnumerable<SpawnGroup>> GetSpawnGroupsAsync();
    Task<IEnumerable<SpawnGroupCollection>> GetSpawnGroupCollectionsAsync();
}
