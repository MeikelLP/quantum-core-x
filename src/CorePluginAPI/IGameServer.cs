using System.Collections.Immutable;
using QuantumCore.API.Game.World;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IGameServer
{
    IWorld World { get; }
    long ServerTime { get; }
    Task CallListener(IConnection connection, IPacketSerializable packet);
    Task RemoveConnection(IConnection connection);
    ImmutableArray<IGameConnection> Connections { get; }
}