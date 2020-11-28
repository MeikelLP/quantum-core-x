using System;
using System.Net.Sockets;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game
{
    public class GameConnection : Connection
    {
        public Server<GameConnection> Server { get; private set; }
        
        public Guid? AccountId { get; set; }
        public string Username { get; set; }
        public PlayerEntity Player { get; set; }

        public GameConnection(Server<GameConnection> server, TcpClient client)
        {
            Server = server;
            Init(client, server);
        }

        protected override void OnHandshakeFinished()
        {
            Server.CallConnectionListener(this);
        }

        protected override void OnClose()
        {
            if (Player?.Map != null)
            {
                Player.Map.DespawnEntity(Player);
            }
            Server.RemoveConnection(this);
            
            // todo enable expiry on auth token
        }

        protected override void OnReceive(object packet)
        {
            Server.CallListener(this, packet);
        }

        protected override long GetServerTime()
        {
            return Server.ServerTime;
        }
    }
}