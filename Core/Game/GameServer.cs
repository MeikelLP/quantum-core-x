using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Types;
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
        private readonly World.World _world;
        
        private readonly Stopwatch _gameTime = new Stopwatch();
        private long _previousTicks = 0;
        private TimeSpan _accumulatedElapsedTime;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        
        public GameServer(GameOptions options)
        {
            _options = options;
            
            // Set public ip address
            if (_options.IpAddress != null)
            {
                IpUtils.PublicIP = IPAddress.Parse(_options.IpAddress);
            }
            else
            {
                // Query interfaces for our best ipv4 address
                IpUtils.SearchPublicIp();
            }
            
            // Initialize static components
            DatabaseManager.Init(options.AccountString, options.GameString);
            CacheManager.Init(options.RedisHost, options.RedisPort);
            
            // Load game data
            Log.Information("Load item_proto");
            var itemProto = ItemProto.FromFile("data/item_proto");
            
            Log.Information("Initialize world"); 
            _world = new World.World();
            _world.Load();
            
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
            _server.RegisterListener<CharacterMove>((connection, packet) => connection.OnCharacterMove(packet));
        }
        
        public async Task Start()
        {
            _server.Start();
            
            _gameTime.Start();

            while (true)
            {
                Tick();
            }
        }

        private void Update(double elapsedTime)
        {
            _world.Update(elapsedTime);
        }

        private void Tick()
        {
            var currentTicks = _gameTime.ElapsedTicks;
            _accumulatedElapsedTime = TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (_accumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                Thread.Sleep((int) sleepTime);
                Tick();
                return;
            }

            if (_accumulatedElapsedTime > _maxElapsedTime)
            {
                _accumulatedElapsedTime = _maxElapsedTime;
            }

            var stepCount = 0;
            while (_accumulatedElapsedTime >= _targetElapsedTime)
            {
                _accumulatedElapsedTime -= _targetElapsedTime;
                ++stepCount;
                
                Update(_targetElapsedTime.TotalMilliseconds);
            }
            
            // todo detect lags
        }
    }
}