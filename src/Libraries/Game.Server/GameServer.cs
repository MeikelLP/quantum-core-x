using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Event;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Game;

public class GameServer : ServerBase<GameConnection>, IGameServer
{
    public static readonly Meter Meter = new Meter("QuantumCore:Game");
    private readonly Histogram<double> _serverTimes = Meter.CreateHistogram<double>("TickTime", "ms");
    private readonly ILogger<GameServer> _logger;
    private readonly PluginExecutor _pluginExecutor;
    private readonly ICommandManager _commandManager;
    private readonly IWorld _world;

    private readonly Stopwatch _gameTime = new Stopwatch();
    private long _previousTicks = 0;
    private TimeSpan _accumulatedElapsedTime;
    private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
    private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
    private readonly Stopwatch _serverTimer = new();

    public new ImmutableArray<IGameConnection> Connections =>
        [..base.Connections.Values.Cast<IGameConnection>()];

    public static GameServer Instance { get; private set; } = null!; // singleton

    public GameServer(
        [FromKeyedServices(HostingOptions.ModeGame)]
        IPacketManager packetManager, ILogger<GameServer> logger,
        PluginExecutor pluginExecutor, IServiceProvider serviceProvider,
        ICommandManager commandManager)
        : base(packetManager, logger, pluginExecutor, serviceProvider, HostingOptions.ModeGame)
    {
        _logger = logger;
        _pluginExecutor = pluginExecutor;
        _commandManager = commandManager;
        Instance = this;
        _world = Scope.ServiceProvider.GetRequiredService<IWorld>();
        Meter.CreateObservableGauge("Connections", () => Connections.Length);
    }

    private void Update(double elapsedTime)
    {
        EventSystem.Update(elapsedTime);

        _world.Update(elapsedTime);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Load game data
        await Task.WhenAll(Scope.ServiceProvider.GetServices<ILoadable>().Select(x => x.LoadAsync(stoppingToken)));

        await _world.InitAsync();

        // Register all default commands
        _commandManager.Register("QuantumCore.Game.Commands", Assembly.GetExecutingAssembly());
        _commandManager.Register("QuantumCore.Game.Commands.Guild", Assembly.GetExecutingAssembly());

        // Put all new connections into login phase
        RegisterNewConnectionListener(connection =>
        {
            connection.SetPhase(EPhase.Login);
            return true;
        });

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
                await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger,
                    x => x.PreUpdateAsync(stoppingToken));
                await Tick();
                await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger,
                    x => x.PostUpdateAsync(stoppingToken));
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
        var elapsedTime = TimeSpan.FromTicks(currentTicks - _previousTicks);
        _serverTimes.Record(elapsedTime.TotalMilliseconds);
        _accumulatedElapsedTime += elapsedTime;
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