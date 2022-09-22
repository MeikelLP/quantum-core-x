using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;

namespace QuantumCore.Auth
{
    public class AuthConnection : Connection
    {
        private AuthServer _server;

        public AuthConnection(AuthServer server, TcpClient client, IPacketManager packetManager, ILogger<AuthConnection> logger) 
            : base(logger)
        {
            _server = server;
            Init(client, packetManager);
        }

        protected override void OnHandshakeFinished()
        {
            _server.CallConnectionListener(this);
        }

        protected override void OnClose()
        {
            _server.RemoveConnection(this);
        }

        protected override void OnReceive(object packet)
        {
            _server.CallListener(this, packet);
        }

        protected override long GetServerTime()
        {
            return _server.ServerTime;
        }
    }
}