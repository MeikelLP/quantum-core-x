using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Event;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Prometheus;
using QuantumCore.Core.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using Serilog;

namespace QuantumCore.Game
{
    internal class GameServer : BackgroundService, IGame 
    {
        public IWorld World => _world;
        public Server<GameConnection> Server => _server;
        
        private readonly GameOptions _options;
        private Server<GameConnection> _server;
        private World.World _world;
        
        private readonly Stopwatch _gameTime = new Stopwatch();
        private long _previousTicks = 0;
        private TimeSpan _accumulatedElapsedTime;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        
        public static GameServer Instance { get; private set; }
        
        public GameServer(IOptions<GameOptions> options)
        {
            Instance = this;
            
            _options = options.Value;
        }

        private void Update(double elapsedTime)
        {
            EventSystem.Update(elapsedTime);
            
            _world.Update(elapsedTime);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

            if (_options.Prometheus)
            {
                // Start metric server
                QuantumCore.Core.Prometheus.Server.Initialize(_options.PrometheusPort);
            }

            // Initialize static components
            DatabaseManager.Init(_options.AccountString, _options.GameString);
            CacheManager.Init(_options.RedisHost, _options.RedisPort);
            
            // Load game configuration
            ConfigManager.Load();
            
            // Start tcp server
            _server = new Server<GameConnection>((server, client) => new GameConnection(server, client), _options.Port);
            
            // Load game data
            Log.Information("Load item_proto");
            ItemManager.Instance.Load();
            Log.Information("Load mob_proto");
            MonsterManager.Load();
            Log.Information("Load jobs.toml");
            JobInfo.Load();
            Log.Information("Load exp.csv");
            ExperienceTable.Load();
            
            // Initialize core systems
            ChatManager.Init();

            // Load all quests
            QuestManager.Init();

            // Load animations
            Log.Information("Load animation data");
            AnimationManager.Load();

            // Load game world
            Log.Information("Initialize world"); 
            _world = new World.World();
            await _world.Load();

            // Load permissions
            Log.Information("Initialize permissions");
            await CommandManager.Load();

            // Register all default commands
            CommandManager.Register("QuantumCore.Game.Commands");

            // Load and init all plugins
            PluginManager.LoadPlugins(this);
            
            // Register game server features
            _server.RegisterNamespace("QuantumCore.Game.Packets");
            
            // Put all new connections into login phase
            _server.RegisterNewConnectionListener(async connection =>
            {
                await connection.SetPhase(EPhases.Login);
                return true;
            });
            
            _server.RegisterListeners();
            
            await _server.Start();
            
            _gameTime.Start();

            Log.Debug("Start!");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Tick();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Tick failed");
                }
            }
        }

        private void Tick()
        {
            var currentTicks = _gameTime.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (_accumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                Thread.Sleep((int) sleepTime);
                return;
            }

            if (_accumulatedElapsedTime > _maxElapsedTime)
            {
                Log.Warning($"Server is running slow");
                _accumulatedElapsedTime = _maxElapsedTime;
            }

            var stepCount = 0;
            while (_accumulatedElapsedTime >= _targetElapsedTime)
            {
                _accumulatedElapsedTime -= _targetElapsedTime;
                ++stepCount;
                
                //Log.Debug($"Update... ({stepCount})");
                Update(_targetElapsedTime.TotalMilliseconds);
            }
            
            // todo detect lags
        }

        public void RegisterCommandNamespace(Type t)
        {
            CommandManager.Register(t.Namespace, t.Assembly);
        }
    }
}