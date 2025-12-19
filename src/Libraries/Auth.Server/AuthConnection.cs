using System.Net.Sockets;
using QuantumCore.API;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Auth;

public class AuthConnection : Connection, IAuthConnection
{
    private readonly IServerBase _server;

    public AuthConnection(IServerBase server, TcpClient client, ILogger<AuthConnection> logger,
        PluginExecutor pluginExecutor, [FromKeyedServices(HostingOptions.MODE_AUTH)] IPacketReader packetReader)
        : base(logger, pluginExecutor, packetReader)
    {
        _server = server;
        Init(client);
    }

    protected override void OnHandshakeFinished()
    {
        _server.CallConnectionListener(this);
    }

    protected override async Task OnClose(bool expected = true)
    {
        await _server.RemoveConnection(this);
    }

    protected override async Task OnReceive(IPacketSerializable packet)
    {
        await _server.CallListener(this, packet);
    }

    protected override ServerClock GetClock()
    {
        return _server.Clock;
    }
}
