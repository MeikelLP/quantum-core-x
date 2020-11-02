using System;
using System.Threading.Tasks;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Networking;
using QuantumCore.Database;

namespace QuantumCore.Game
{
    internal class GameServer : IServer
    {
        private readonly GameOptions _options;
        private readonly Server _server;
        
        public GameServer(GameOptions options)
        {
            _options = options;
            
            // Initialize static components
            DatabaseManager.Init(options.AccountString, options.GameString);
            CacheManager.Init(options.RedisHost, options.RedisPort);
            
            // Start tcp server
            _server = new Server(options.Port);
            
            // Load and init all plugins
            PluginManager.LoadPlugins();
        }
        
        public async Task Start()
        {
            await _server.Start();
        }
    }
}