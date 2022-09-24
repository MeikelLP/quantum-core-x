using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;

namespace QuantumCore.Auth
{
    public class AuthConnection : Connection
    {
        private AuthServer _server;

        public AuthConnection(AuthServer server, TcpClient client, IPacketManager packetManager, 
            ILogger<AuthConnection> logger, PluginExecutor pluginExecutor) 
            : base(logger, pluginExecutor, packetManager)
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

        protected async override Task OnReceive(object packet)
        {
            await _server.CallListener(this, packet);
        }

        protected override long GetServerTime()
        {
            return _server.ServerTime;
        }
    }
}