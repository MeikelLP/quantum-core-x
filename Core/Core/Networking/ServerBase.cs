using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Packets;
using QuantumCore.Extensions;
using Weikio.PluginFramework.Abstractions;

namespace QuantumCore.Core.Networking
{
    public abstract class ServerBase<T> : BackgroundService where T : IConnection
    {
        private readonly ILogger _logger;
        protected IPacketManager PacketManager { get; }
        private readonly List<Func<T, Task<bool>>> _connectionListeners = new();
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new();
        private readonly Dictionary<ushort, IPacketHandler> _listeners = new();
        private readonly Stopwatch _serverTimer = new();
        private readonly CancellationTokenSource _stoppingToken = new();
        protected TcpListener Listener { get; }

        private ServiceProvider _serverLifetimeProvider;
        private readonly PluginExecutor _pluginExecutor;
        private readonly IEnumerable<IPacketHandler> _packetHandlers;
        protected IServiceCollection Services { get; }

        public int Port { get; }

        public ServerBase(IPacketManager packetManager, ILogger logger, PluginExecutor pluginExecutor,
            IServiceProvider serviceProvider, IEnumerable<IPacketHandler> packetHandlers,
            int port, string bindIp = "0.0.0.0")
        {
            _logger = logger;
            _pluginExecutor = pluginExecutor;
            _packetHandlers = packetHandlers;
            PacketManager = packetManager;
            Port = port;
            
            // Start server timer
            _serverTimer.Start();
            
            var localAddr = IPAddress.Parse(bindIp);
            Listener = new TcpListener(localAddr, Port);

            _logger.LogInformation("Initialize tcp server listening on {IP}:{Port}", bindIp, port);

            // Register Core Features
            PacketManager.RegisterNamespace("QuantumCore.Core.Packets");
            var cfg = serviceProvider.GetRequiredService<IConfiguration>();
            Services = new ServiceCollection()
                .AddCoreServices(serviceProvider.GetRequiredService<IPluginCatalog>())
                .AddSingleton(_ => cfg)
                .Replace(new ServiceDescriptor(typeof(IPacketManager), _ => packetManager, ServiceLifetime.Singleton));
        }

        public long ServerTime => _serverTimer.ElapsedMilliseconds;

        public async Task RemoveConnection(IConnection connection)
        {
            _connections.Remove(connection.Id, out _);
            
            await _pluginExecutor.ExecutePlugins<IConnectionLifetimeListener>(_logger, x => x.OnDisconnectedAsync(_stoppingToken.Token));
        }

        public override Task StartAsync(CancellationToken token)
        {
            base.StartAsync(token);
            _logger.LogInformation("Start listening for connections...");

            _serverLifetimeProvider = Services.BuildServiceProvider();
            Listener.Start();
            Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);

            return Task.CompletedTask;
        }

        private async void OnClientAccepted(IAsyncResult ar)
        {
            var listener = (TcpListener) ar.AsyncState;
            var client = listener!.EndAcceptTcpClient(ar);
            
            // will dispose once connection finished executing (canceled or disconnect) 
            await using var scope = _serverLifetimeProvider.CreateAsyncScope();

            // cannot inject tcp client here
            var connection = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, client);
            _connections.TryAdd(connection.Id, connection);
            
            await _pluginExecutor.ExecutePlugins<IConnectionLifetimeListener>(_logger, x => x.OnConnectedAsync(_stoppingToken.Token));

            // accept new connections on another thread
            Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);
            
            await connection.StartAsync(_stoppingToken.Token);
            await connection.ExecuteTask.ConfigureAwait(false);
        }

        public async Task ForAllConnections(Func<IConnection, Task> callback)
        {
            foreach (var connection in _connections.Values)
            {
                await callback(connection);
            }
        }

        public void RegisterNewConnectionListener(Func<T, Task<bool>> listener)
        {
            _connectionListeners.Add(listener);
        }

        public async Task CallListener(IConnection connection, object packet)
        {
            var header = PacketManager.IncomingPackets.First(p => p.Value.Type == packet.GetType());
            if (!_listeners.ContainsKey(header.Key))
            {
                _logger.LogWarning("Don't know how to handle header {Header}", header.Key);
                return;
            }

            var handler = _listeners[header.Key];
            var handlerType = handler.GetType();
            var packetType = handlerType.GetPacketType();
            
            // TODO caching
            var handlerExecuteMethod =
                    handlerType.GetMethod(nameof(ISelectPacketHandler<object>.ExecuteAsync))!;
            var contextPacketProperty = typeof(PacketContext<>).MakeGenericType(packetType)
                .GetProperty(nameof(PacketContext<object>.Packet))!;
            var contextConnectionProperty = typeof(PacketContext<>).MakeGenericType(packetType)
                .GetProperty(nameof(PacketContext<object>.Connection))!;

            var context = Activator.CreateInstance(typeof(PacketContext<>).MakeGenericType(packetType));
            contextPacketProperty.SetValue(context, packet);
            contextConnectionProperty.SetValue(context, connection);

            try
            {
                await (Task) handlerExecuteMethod.Invoke(handler, new[] { context, new CancellationToken() })!;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute packet handler");
            }
        }

        public void CallConnectionListener(T connection)
        {
            foreach (var listener in _connectionListeners) listener(connection);
        }

        protected void StartListening()
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(OnClientAccepted, Listener);
        }

        public void RegisterListeners()
        {
            foreach (var packetHandler in _packetHandlers)
            {
                var packetType = packetHandler.GetType().GetPacketType();

                if (packetType is null)
                {
                    _logger.LogWarning("Base interface did not match {BaseInterface} this should not happen", nameof(IPacketHandler));
                    continue;
                }
                
                var packetDescription = packetType.GetCustomAttribute<PacketAttribute>();
                if (packetDescription is null)
                {
                    _logger.LogWarning("Packet type {Type} is missing a {AttributeTypeName}", packetType.Name, nameof(PacketAttribute));
                    continue;
                }
                
                var subPacketDescription = packetType.GetCustomAttribute<SubPacketAttribute>();
                if (subPacketDescription is not null)
                {
                    _listeners.Add((ushort)(packetDescription.Header << 8 | subPacketDescription.SubHeader), packetHandler);
                }
                else
                {
                    _listeners.Add(packetDescription.Header, packetHandler);
                }
            }
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingToken.Cancel();
            await base.StopAsync(cancellationToken);
        }
    }
}