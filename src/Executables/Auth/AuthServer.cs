using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Caching;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;

namespace QuantumCore.Auth
{
    public class AuthServer : ServerBase<AuthConnection>
    {
        private readonly ILogger<AuthServer> _logger;
        private readonly ICacheManager _cacheManager;

        public AuthServer(IOptions<HostingOptions> hostingOptions, ILogger<AuthServer> logger,
            PluginExecutor pluginExecutor, IServiceProvider serviceProvider, ICacheManager cacheManager)
            : base(logger, pluginExecutor, serviceProvider, "auth", hostingOptions)
        {
            _logger = logger;
            _cacheManager = cacheManager;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            // Register auth server features
            RegisterNewConnectionListener(NewConnection);

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