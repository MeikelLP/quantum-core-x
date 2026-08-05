using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.PluginTypes;
using QuantumCore.Networking;

namespace QuantumCore.Core.Networking;

public abstract class ServerBase<T> : BackgroundService, IServerBase
    where T : IConnection
{
    private readonly ILogger _logger;
    protected IPacketManager PacketManager { get; }
    private readonly List<Func<IConnection, bool>> _connectionListeners = [];
    protected ConcurrentDictionary<Guid, IConnection> Connections { get; } = new();
    private readonly CancellationTokenSource _stoppingToken = new();
    protected TcpListener Listener { get; }

    private readonly PluginExecutor _pluginExecutor;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _serverMode;
    protected IServiceScope Scope { get; }

    public ServerClock Clock { get; }
    public ushort Port { get; }
    public IPAddress IpAddress { get; }

    protected ServerBase(IPacketManager packetManager, ILogger logger, PluginExecutor pluginExecutor,
        IServiceProvider serviceProvider, ServerClock clock, string mode)
    {
        _logger = logger;
        _pluginExecutor = pluginExecutor;
        _serviceProvider = serviceProvider;
        _serverMode = mode;
        PacketManager = packetManager;
        Scope = serviceProvider.CreateScope();
        var hostingOptions = Scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<HostingOptions>>().Get(mode);
        Port = hostingOptions.Port;

        // Start server timer
        Clock = clock;
        var desiredIpAddress = IPAddress.TryParse(hostingOptions.IpAddress, out var ipAddress)
            ? ipAddress
            : IPAddress.Loopback;
        IpAddress = desiredIpAddress;
        Listener = new TcpListener(IpAddress, Port);
    }

    public async Task RemoveConnectionAsync(IConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        Connections.Remove(connection.Id, out _);

        await _pluginExecutor.ExecutePluginsAsync<IConnectionLifetimeListener>(_logger,
            x => x.OnDisconnectedAsync(_stoppingToken.Token));
    }

    public async override Task StartAsync(CancellationToken token)
    {
        await base.StartAsync(token);

        Listener.Start();
        Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);

        _logger.LogInformation("Start listening for connections on {IP}:{Port} ({Mode})", IpAddress, Port,
            _serverMode);
    }

#pragma warning disable VSTHRD100 // do not use async void - how to use async Task with tcp accept tcp client?
    private async void OnClientAccepted(IAsyncResult ar)
#pragma warning restore VSTHRD100
    {
        try
        {
            var listener = (TcpListener) ar.AsyncState!;
            var client = listener.EndAcceptTcpClient(ar);

            // will dispose once connection finished executing (canceled or disconnect)
            await using var scope = _serviceProvider.CreateAsyncScope();

            // cannot inject tcp client here
            var connection = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, client, this);
            Connections.TryAdd(connection.Id, connection);

            await _pluginExecutor.ExecutePluginsAsync<IConnectionLifetimeListener>(_logger,
                x => x.OnConnectedAsync(_stoppingToken.Token));

            // accept new connections on another thread
            Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);

            await connection.StartAsync(_stoppingToken.Token);
            if (connection.ExecuteTask is not null)
            {
                await connection.ExecuteTask.ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to accept TCP connection");
        }
    }

    public void ForAllConnections(Action<IConnection> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        foreach (var connection in Connections.Values)
        {
            callback(connection);
        }
    }

    public void RegisterNewConnectionListener(Func<IConnection, bool> listener)
    {
        _connectionListeners.Add(listener);
    }

    public async Task CallListenerAsync(IConnection connection, IPacketSerializable packet)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(packet);
        if (!PacketManager.TryGetPacketInfo(packet, out var details) || details.PacketHandlerType is null)
        {
            _logger.LogWarning("Could not find a handler for packet {PacketType}", packet.GetType());
            return;
        }

        object context;
        if (_serverMode == HostingOptions.MODE_GAME)
        {
            context = GetGameContextPacket(connection, packet, details.PacketType);
        }
        else if (_serverMode == HostingOptions.MODE_AUTH)
        {
            context = GetAuthContextPacket(connection, packet, details.PacketType);
        }
        else
        {
            throw new ArgumentException("Unknown server mode");
        }

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var packetHandler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, details.PacketHandlerType);
            var handlerExecuteMethod = details.PacketHandlerType.GetMethod("ExecuteAsync")!;
            await (Task) handlerExecuteMethod.Invoke(packetHandler, [context, new CancellationToken()])!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute packet handler");
            connection.Close();
        }
    }

    private static object GetGameContextPacket(IConnection connection, object packet, Type packetType)
    {
        // TODO caching
        var contextPacketProperty = typeof(GamePacketContext<>).MakeGenericType(packetType)
            .GetProperty(nameof(GamePacketContext<>.Packet))!;
        var contextConnectionProperty = typeof(GamePacketContext<>).MakeGenericType(packetType)
            .GetProperty(nameof(GamePacketContext<>.Connection))!;

        var context = Activator.CreateInstance(typeof(GamePacketContext<>).MakeGenericType(packetType))!;
        contextPacketProperty.SetValue(context, packet);
        contextConnectionProperty.SetValue(context, connection);
        return context;
    }

    private static object GetAuthContextPacket(IConnection connection, object packet, Type packetType)
    {
        // TODO caching
        var contextPacketProperty = typeof(AuthPacketContext<>).MakeGenericType(packetType)
            .GetProperty(nameof(AuthPacketContext<>.Packet))!;
        var contextConnectionProperty = typeof(AuthPacketContext<>).MakeGenericType(packetType)
            .GetProperty(nameof(AuthPacketContext<>.Connection))!;

        var context = Activator.CreateInstance(typeof(AuthPacketContext<>).MakeGenericType(packetType))!;
        contextPacketProperty.SetValue(context, packet);
        contextConnectionProperty.SetValue(context, connection);
        return context;
    }

    public void CallConnectionListener(IConnection connection)
    {
        foreach (var listener in _connectionListeners) listener(connection);
    }

    protected void StartListening()
    {
        Listener.Start();
        Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);
    }

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
        await _stoppingToken.CancelAsync();
        await base.StopAsync(cancellationToken);
        _stoppingToken.Dispose();
        Scope.Dispose();
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        _stoppingToken.Dispose();
    }
}