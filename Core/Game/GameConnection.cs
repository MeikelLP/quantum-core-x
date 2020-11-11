using System;
using System.Net.Sockets;
using QuantumCore.Core.Networking;

namespace QuantumCore.Game
{
    public class GameConnection : Connection
    {
        private Server<GameConnection> _server;
        
        public Guid AccountId { get; set; }
        public string Username { get; set; }

        public GameConnection(Server<GameConnection> server, TcpClient client)
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