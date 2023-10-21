using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Auth
{
    public class AuthServer : ServerBase<AuthConnection>
    {
        private readonly ILogger<AuthServer> _logger;
        private readonly ICacheManager _cacheManager;

        public AuthServer(IOptions<HostingOptions> hostingOptions, IPacketManager packetManager, ILogger<AuthServer> logger,
            PluginExecutor pluginExecutor, IServiceProvider serviceProvider,
            IEnumerable<IPacketHandler> packetHandlers, ICacheManager cacheManager)
            : base(packetManager, logger, pluginExecutor, serviceProvider, packetHandlers, "auth", hostingOptions)
        {
            _logger = logger;
            _cacheManager = cacheManager;
        }

        protected async override Task ExecuteAsync(CancellationToken token)
        {
            // Register auth server features
            RegisterNewConnectionListener(NewConnection);
            RegisterListeners();

            var pong = await _cacheManager.Ping();
            if (!pong)
            {
                _logger.LogError("Failed to ping redis server");
            }
        }

        private bool NewConnection(IConnection connection)
        {
            connection.SetPhase(EPhases.Auth);
            return true;
        }
    }
}
