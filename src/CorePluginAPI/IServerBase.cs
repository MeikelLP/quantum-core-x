using System.Net;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnection(IConnection connection);
    Task CallListener(IConnection connection, IPacketSerializable packet);
    long ServerTime { get; }
    IPAddress IpAddress { get; }
    ushort Port { get; }
    void CallConnectionListener(IConnection connection);
}
