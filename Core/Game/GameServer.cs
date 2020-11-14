using System;
using System.Net;
using System.Threading.Tasks;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
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
            if (_options.IpAddress != null)
            {
                IpUtils.PublicIP = IPAddress.Parse(_options.IpAddress);
            }
            else
            {
                IpUtils.SearchPublicIp();
            }
            
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
            
            _server.RegisterListener<TokenLogin>((connection, packet) => connection.OnTokenLogin(packet));
            _server.RegisterListener<CreateCharacter>((connection, packet) => connection.OnCreateCharacter(packet));
            _server.RegisterListener<SelectCharacter>((connection, packet) => connection.OnSelectCharacter(packet));
            _server.RegisterListener<EnterGame>((connection, packet) => connection.OnEnterGame(packet));
        }
        
        public async Task Start()
        {
            await _server.Start();
        }
    }
}