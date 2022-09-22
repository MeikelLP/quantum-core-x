using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using QuantumCore.Auth;
using QuantumCore.Core.Networking;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game
{
    public class GameConnection : Connection
    {
        public GameServer Server { get; }
        public Guid? AccountId { get; set; }
        public string Username { get; set; }
        public PlayerEntity Player { get; set; }

        public GameConnection(GameServer server, TcpClient client, IPacketManager packetManager, ILogger<AuthConnection> logger) 
            : base(logger)
        {
            Server = server;
            Init(client, packetManager);
        }

        protected override void OnHandshakeFinished()
        {
            GameServer.Instance.CallConnectionListener(this);
        }

        protected override void OnClose()
        {
            if (Player != null)
            {
                World.World.Instance.DespawnEntity(Player);
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