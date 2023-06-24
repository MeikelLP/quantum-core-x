using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Networking;

namespace QuantumCore.Game
{
    public class GameConnection : Connection, IGameConnection
    {
        private readonly IWorld _world;
        public IServerBase Server { get; }
        public Guid? AccountId { get; set; }
        public string Username { get; set; }
        public IPlayerEntity Player { get; set; }

        public GameConnection(IServerBase server, TcpClient client, IPacketManager packetManager, 
            ILogger<GameConnection> logger, PluginExecutor pluginExecutor, IWorld world) 
            : base(logger, pluginExecutor, packetManager)
        {
            _world = world;
            Server = server;
            Init(client);
        }

        protected override void OnHandshakeFinished()
        {
            GameServer.Instance.CallConnectionListener(this);
        }

        protected async override Task OnClose()
        {
            if (Player != null)
            {
                await _world.DespawnEntity(Player);
            }

            await Server.RemoveConnection(this);
            
            // todo enable expiry on auth token
        }

        protected async override Task OnReceive(object packet)
        {
            await Server.CallListener(this, packet);
        }

        protected override long GetServerTime()
        {
            return Server.ServerTime;
        }
    }
}