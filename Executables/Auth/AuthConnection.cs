using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Auth
{
    public class AuthConnection : Connection, IAuthConnection
    {
        private readonly AuthServer _server;

        public AuthConnection(AuthServer server, TcpClient client, ILogger<AuthConnection> logger, 
            PluginExecutor pluginExecutor, IPacketReader packetReader) 
            : base(logger, pluginExecutor, packetReader)
        {
            _server = server;
            Init(client);
        }

        protected override void OnHandshakeFinished()
        {
            _server.CallConnectionListener(this);
        }

        protected async override Task OnClose()
        {
            await _server.RemoveConnection(this);
        }

        protected async override Task OnReceive(IPacketSerializable packet)
        {
            await _server.CallListener(this, packet);
        }

        protected override long GetServerTime()
        {
            return _server.ServerTime;
        }
    }
}