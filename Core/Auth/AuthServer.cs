using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Auth
{
    public class AuthServer : ServerBase<AuthConnection>
    {
        private readonly ILogger<AuthServer> _logger;
        private readonly AuthOptions _options;
        private readonly IDatabaseManager _databaseManager;
        private readonly ICacheManager _cacheManager;

        public AuthServer(IOptions<AuthOptions> options, IPacketManager packetManager, ILogger<AuthServer> logger, 
            PluginExecutor pluginExecutor, IServiceProvider serviceProvider, IDatabaseManager databaseManager, 
            IEnumerable<IPacketHandler> packetHandlers, ICacheManager cacheManager)
            : base(packetManager, logger, pluginExecutor, serviceProvider, packetHandlers, options.Value.Port)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _cacheManager = cacheManager;
            _options = options.Value;
            
            Services.AddSingleton(_ => this);
        }

        protected async override Task ExecuteAsync(CancellationToken token)
        {
            // Initialize static components
            _databaseManager.Init(_options.AccountString, _options.GameString);

            // Register auth server features
            PacketManager.RegisterNamespace("QuantumCore.Auth.Packets");
            RegisterNewConnectionListener(NewConnection);
            
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