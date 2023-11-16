using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Event;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Game.Commands;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Networking;

namespace QuantumCore.Game
{
    public class GameServer : ServerBase<GameConnection>, IGame, IGameServer
    {
        private readonly HostingOptions _hostingOptions;
        private readonly ILogger<GameServer> _logger;
        private readonly PluginExecutor _pluginExecutor;
        private readonly IItemManager _itemManager;
        private readonly IMonsterManager _monsterManager;
        private readonly IExperienceManager _experienceManager;
        private readonly IAnimationManager _animationManager;
        private readonly ICommandManager _commandManager;
        private readonly IQuestManager _questManager;
        private readonly IChatManager _chatManager;
        public IWorld World { get; }

        private readonly Stopwatch _gameTime = new Stopwatch();
        private long _previousTicks = 0;
        private TimeSpan _accumulatedElapsedTime;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        private readonly Stopwatch _serverTimer = new();

        public static GameServer Instance { get; private set; } = null!; // singleton

        public GameServer(IOptions<HostingOptions> hostingOptions, IPacketManager packetManager,
            ILogger<GameServer> logger, PluginExecutor pluginExecutor, IServiceProvider serviceProvider,
            IItemManager itemManager, IMonsterManager monsterManager, IExperienceManager experienceManager,
            IAnimationManager animationManager, ICommandManager commandManager,
            IEnumerable<IPacketHandler> packetHandlers, IQuestManager questManager, IChatManager chatManager,
            IWorld world)
            : base(packetManager, logger, pluginExecutor, serviceProvider, packetHandlers, "game", hostingOptions)
        {
            _hostingOptions = hostingOptions.Value;
            _logger = logger;
            _pluginExecutor = pluginExecutor;
            _itemManager = itemManager;
            _monsterManager = monsterManager;
            _experienceManager = experienceManager;
            _animationManager = animationManager;
            _commandManager = commandManager;
            _questManager = questManager;
            _chatManager = chatManager;
            World = world;
            Instance = this;
        }

        private void Update(double elapsedTime)
        {
            EventSystem.Update(elapsedTime);

            World.Update(elapsedTime);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Set public ip address
            if (_hostingOptions.IpAddress != null)
            {
                IpUtils.PublicIP = IPAddress.Parse(_hostingOptions.IpAddress);
            }
            else if(IpUtils.PublicIP is null)
            {
                // Query interfaces for our best ipv4 address
                IpUtils.SearchPublicIp();
            }

            // Load game data
            await Task.WhenAll(
                _itemManager.LoadAsync(stoppingToken),
                _monsterManager.LoadAsync(stoppingToken),
                _experienceManager.LoadAsync(stoppingToken),
                _animationManager.LoadAsync(stoppingToken),
                _commandManager.LoadAsync(stoppingToken)
            );

            // Initialize core systems
            _chatManager.Init();

            // Load all quests
            _questManager.Init();

            // Load game world
            _logger.LogInformation("Initialize world");
            await World.Load();

            // Register all default commands
            _commandManager.Register("QuantumCore.Game.Commands");

            // Put all new connections into login phase
            RegisterNewConnectionListener(connection =>
            {
                connection.SetPhase(EPhases.Login);
                return true;
            });

            RegisterListeners();

            // Start server timer
            _serverTimer.Start();

            _logger.LogInformation("Start listening for connections...");

            StartListening();

            _gameTime.Start();

            _logger.LogDebug("Start!");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger, x => x.PreUpdateAsync(stoppingToken));
                    await Tick();
                    await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger, x => x.PostUpdateAsync(stoppingToken));
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
            _commandManager.Register(t.Namespace!, t.Assembly);
        }
    }
}
