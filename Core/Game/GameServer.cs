using System;
using System.Threading.Tasks;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using Serilog;

namespace QuantumCore.Game
{
    internal class GameServer : IServer
    {
        private readonly GameOptions _options;
        private readonly Server<GameConnection> _server;
        
        public GameServer(GameOptions options)
        {
            _options = options;
            
            // Initialize static components
            DatabaseManager.Init(options.AccountString, options.GameString);
            CacheManager.Init(options.RedisHost, options.RedisPort);
            
            // Start tcp server
            _server = new Server<GameConnection>((server, client) => new GameConnection(server, client), options.Port);
            
            // Load and init all plugins
            PluginManager.LoadPlugins();
            
            // Register game server features
            _server.RegisterNamespace("QuantumCore.Game.Packets");
            
            _server.RegisterNewConnectionListener(connection =>
            {
                connection.SetPhase(EPhases.Login);
                return true;
            });
            
            _server.RegisterListener<TokenLogin>(OnTokenLogin);
        }

        private async void OnTokenLogin(Connection connection, TokenLogin packet)
        {
            var key = "token:" + packet.Key;

            if (await CacheManager.Redis.Exists(key) <= 0)
            {
                Log.Warning($"Received invalid auth token {packet.Key} / {packet.Username}");
                connection.Close();
                return;
            }
            
            var username = await CacheManager.Redis.Get<string>("token:" + packet.Key);
            if (!string.Equals(username, packet.Username, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning($"Received invalid auth token, username does not match {username} != {packet.Username}");
                connection.Close();
                return;
            }
            
            Log.Debug("Received valid auth token");
            
            // Remove TTL from token so we can use it for another game core transition
            await CacheManager.Redis.Persist(key);
        }

        public async Task Start()
        {
            await _server.Start();
        }
    }
}