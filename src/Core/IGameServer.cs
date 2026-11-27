using System.Collections.Immutable;

namespace QuantumCore.API;

public interface IGameServer
{
    ImmutableArray<IGameConnection> Connections { get; }
}