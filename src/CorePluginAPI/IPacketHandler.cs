using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IPacketHandler
{
}

public interface IPacketHandler<TConnection, TPacket> : IPacketHandler
    where TPacket : IPacket
    where TConnection : IConnection
{
    ValueTask ExecuteAsync(PacketContext<TConnection, TPacket> context, CancellationToken token = default);
}
