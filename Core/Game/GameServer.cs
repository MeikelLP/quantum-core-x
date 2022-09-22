using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Cache;
using QuantumCore.Core.API;
using QuantumCore.Core.Event;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;

namespace QuantumCore.Game
{
    public class GameServer : ServerBase<GameConnection>, IGame
    {
        private readonly ILogger<GameServer> _logger;
        public IWorld World => _world;
        private readonly GameOptions _options;
        private World.World _world;

        private readonly Stopwatch _gameTime = new Stopwatch();
        private long _previousTicks = 0;
        private TimeSpan _accumulatedElapsedTime;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        private readonly Stopwatch _serverTimer = new();

        private readonly Gauge _openConnections = Metrics.CreateGauge("open_connections", "Currently open connections");

        public static GameServer Instance { get; private set; }
        
        public GameServer(IOptions<GameOptions> options, IServiceProvider serviceProvider, IPacketManager packetManager, ILogger<GameServer> logger) 
            : base(serviceProvider, packetManager, logger, options.Value.Port)
        {
            _logger = logger;
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
            
            // Load game data
            _logger.LogInformation("Load item_proto");
            ItemManager.Instance.Load();
            _logger.LogInformation("Load mob_proto");
            MonsterManager.Load();
            _logger.LogInformation("Load jobs.toml");
            JobInfo.Load();
            _logger.LogInformation("Load exp.csv");
            ExperienceTable.Load();
            
            // Initialize core systems
            ChatManager.Init();

            // Load all quests
            QuestManager.Init();

            // Load animations
            _logger.LogInformation("Load animation data");
            AnimationManager.Load();

            // Load game world
            _logger.LogInformation("Initialize world"); 
            _world = new World.World();
            await _world.Load();

            // Load permissions
            _logger.LogInformation("Initialize permissions");
            await CommandManager.Load();

            // Register all default commands
            CommandManager.Register("QuantumCore.Game.Commands");

            // Load and init all plugins
            PluginManager.LoadPlugins(this);
            
            // Register game server features
            PacketManager.RegisterNamespace("QuantumCore.Game.Packets");
            
            // Put all new connections into login phase
            RegisterNewConnectionListener(async connection =>
            {
                await connection.SetPhase(EPhases.Login);
                return true;
            });
            
            RegisterListeners<GameConnection>();
            
            // Start server timer
            _serverTimer.Start();

            // Register Core Features
            PacketManager.RegisterNamespace("QuantumCore.Core.Packets");
            RegisterListener<GCHandshake>((connection, packet) => connection.HandleHandshake(packet));
            _logger.LogInformation("Start listening for connections...");

            StartListening();
            
            _gameTime.Start();

            _logger.LogDebug("Start!");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Tick();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Tick failed");
                }
            }
        }

        private async ValueTask Tick()
        {
            var currentTicks = _gameTime.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (_accumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (_targetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                await Task.Delay((int) sleepTime).ConfigureAwait(false);
                return;
            }

            if (_accumulatedElapsedTime > _maxElapsedTime)
            {
                _logger.LogWarning($"Server is running slow");
                _accumulatedElapsedTime = _maxElapsedTime;
            }

            var stepCount = 0;
            while (_accumulatedElapsedTime >= _targetElapsedTime)
            {
                _accumulatedElapsedTime -= _targetElapsedTime;
                ++stepCount;
                
                //_logger.LogDebug($"Update... ({stepCount})");
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