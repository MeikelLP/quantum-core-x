using System.Collections.Immutable;

namespace QuantumCore;

public interface IGameServer
{
    ImmutableArray<IGameConnection> Connections { get; }
}