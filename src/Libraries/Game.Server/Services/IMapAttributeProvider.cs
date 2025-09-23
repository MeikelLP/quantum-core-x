using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Services;

public interface IMapAttributeProvider
{
    Task<IMapAttributeSet?> GetAttributesAsync(string mapName, Coordinates position, uint mapWidth, uint mapHeight,
        CancellationToken cancellationToken = default);
}

public interface IMapAttributeSet
{
    EMapAttribute GetAttribute(Coordinates coords);
}
