using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Auth
{
    public class AuthServer : ServerBase<AuthConnection>
    {
        private readonly ILogger<AuthServer> _logger;
        private readonly AuthOptions _options;
        private readonly ICacheManager _cacheManager;

        public AuthServer(IOptions<AuthOptions> options, IConfiguration configuration, IPacketManager packetManager, ILogger<AuthServer> logger, 
            PluginExecutor pluginExecutor, IServiceProvider serviceProvider, 
            IEnumerable<IPacketHandler> packetHandlers, ICacheManager cacheManager)
            : base(packetManager, logger, pluginExecutor, serviceProvider, packetHandlers, configuration.GetValue<string>("Mode"), options.Value.Port)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _options = options.Value;
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

        private async Task<bool> NewConnection(IConnection connection)
        {
            await connection.SetPhaseAsync(EPhases.Auth);
            return true;
        }
    }
}