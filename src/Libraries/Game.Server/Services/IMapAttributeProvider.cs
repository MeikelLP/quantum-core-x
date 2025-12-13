using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Services;

public interface IMapAttributeProvider
{
    Task<IMapAttributeSet?> GetAttributesAsync(string mapName, Coordinates position, uint mapWidth, uint mapHeight,
        CancellationToken cancellationToken = default);
}

public interface IMapAttributeSet
{
    EMapAttributes GetAttributesAt(Coordinates coords);
}
