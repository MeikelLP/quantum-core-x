using System.Net;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnection(IConnection connection);
    Task CallListener(IConnection connection, IPacketSerializable packet);
    ServerClock Clock { get; }
    IPAddress IpAddress { get; }
    ushort Port { get; }
    void CallConnectionListener(IConnection connection);
}
