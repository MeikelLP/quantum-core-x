using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Timekeeping;
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

    private ServerTimestamp _lastTick;
    private TimeSpan _accumulatedElapsedTime;
    private readonly TimeSpan _targetElapsedTime = TimeSpan.FromTicks(100000); // 100hz
    private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

    public new ImmutableArray<IGameConnection> Connections =>
        [..base.Connections.Values.Cast<IGameConnection>()];

    public static GameServer Instance { get; private set; } = null!; // singleton

    public GameServer(
        [FromKeyedServices(HostingOptions.MODE_GAME)]
        IPacketManager packetManager, ILogger<GameServer> logger,
        PluginExecutor pluginExecutor, IServiceProvider serviceProvider, ServerClock clock,
        ICommandManager commandManager)
        : base(packetManager, logger, pluginExecutor, serviceProvider, clock, HostingOptions.MODE_GAME)
    {
        _logger = logger;
        _pluginExecutor = pluginExecutor;
        _commandManager = commandManager;
        Instance = this;
        _world = Scope.ServiceProvider.GetRequiredService<IWorld>();
        _lastTick = Clock.Now;
        Meter.CreateObservableGauge("Connections", () => Connections.Length);
    }

    private void Update(TickContext ctx)
    {
        EventSystem.Update(ctx);

        _world.Update(ctx);
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
            connection.SetPhase(EPhase.LOGIN);
            return true;
        });

        _logger.LogInformation("Start listening for connections...");

        StartListening();

        _lastTick = Clock.Now;

        _logger.LogDebug("Start!");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger,
                    x => x.PreUpdateAsync(stoppingToken));
                await Tick(stoppingToken);
                await _pluginExecutor.ExecutePlugins<IGameTickListener>(_logger,
                    x => x.PostUpdateAsync(stoppingToken));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Tick failed");
            }
        }
    }

    private async ValueTask Tick(CancellationToken stoppingToken)
    {
        var currentTick = Clock.Now;
        var elapsedTime = Clock.ElapsedBetween(_lastTick, currentTick);
        _lastTick = currentTick;

        _serverTimes.Record(elapsedTime.TotalMilliseconds);
        _accumulatedElapsedTime += elapsedTime;

        if (_accumulatedElapsedTime < _targetElapsedTime)
        {
            var sleepTime = _targetElapsedTime - _accumulatedElapsedTime;
            await Task.Delay(sleepTime, Clock.TimeProvider, stoppingToken).ConfigureAwait(false);
            return;
        }

        // Spiral of death prevention: https://gafferongames.com/post/fix_your_timestep/
        // clamp at a maximum elapsed interval per tick
        if (_accumulatedElapsedTime > _maxElapsedTime)
        {
            _logger.LogWarning("Server is running slow: tick delayed by {TotalMilliseconds:F2}ms",
                (_accumulatedElapsedTime - _maxElapsedTime).TotalMilliseconds);
            _accumulatedElapsedTime = _maxElapsedTime;
        }

        var stepCount = 0;

        // Catch-up loop: may run multiple fixed updates if we fell behind
        while (_accumulatedElapsedTime >= _targetElapsedTime)
        {
            _accumulatedElapsedTime -= _targetElapsedTime;
            ++stepCount;

            var stepTimestamp = Clock.Rewind(currentTick, _accumulatedElapsedTime);

            //_logger.LogDebug($"Update... ({stepCount})");
            Update(new TickContext(Clock, _targetElapsedTime, stepTimestamp));
        }

        // todo detect lags
    }

    public void RegisterCommandNamespace(Type t)
    {
        _commandManager.Register(t.Namespace!, t.Assembly);
    }
}
