using System.Reflection;

namespace QuantumCore;

/// <summary>
/// This will be registered as a keyed service.
/// Used to define where to load packet types and handlers from
/// </summary>
public interface IPacketLocationProvider
{
    IReadOnlyCollection<Assembly> GetPacketAssemblies();
}
