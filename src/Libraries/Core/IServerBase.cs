using System.Net;
using QuantumCore.Networking;
using QuantumCore.Timekeeping;

namespace QuantumCore;

public interface IServerBase
{
    void RemoveConnection(IConnection connection);
    Task CallListenerAsync(IConnection connection, IPacketSerializable packet);
    ServerClock Clock { get; }
    IPAddress IpAddress { get; }
    ushort Port { get; }
    void CallConnectionListener(IConnection connection);
}