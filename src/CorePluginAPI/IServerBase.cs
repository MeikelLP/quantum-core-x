using System.Net;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnectionAsync(IConnection connection);
    Task CallListenerAsync(IConnection connection, IPacketSerializable packet);
    ServerClock Clock { get; }
    IPAddress IpAddress { get; }
    ushort Port { get; }
    void CallConnectionListener(IConnection connection);
}