using System.Net.Sockets;
using QuantumCore.Core.Networking;

namespace QuantumCore.Auth
{
    public class AuthConnection : Connection
    {
        private Server<AuthConnection> _server;

        public AuthConnection(Server<AuthConnection> server, TcpClient client)
        {
            _server = server;
            Init(client, server);
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