using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Services;

public interface IAtlasProvider
{
    Task<IEnumerable<IMap>> GetAsync(IWorld world);
}