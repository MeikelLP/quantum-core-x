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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using QuantumCore.API;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;

namespace QuantumCore.Core.Networking
{
    public abstract class ServerBase<T> : BackgroundService where T : Connection
    {
        private readonly ILogger _logger;
        protected IPacketManager PacketManager { get; }
        private readonly List<Func<T, Task<bool>>> _connectionListeners = new();
        private readonly ConcurrentDictionary<Guid, IConnection> _connections = new();
        private readonly Dictionary<ushort, Delegate> _listeners = new();
        private readonly Stopwatch _serverTimer = new();
        private readonly CancellationTokenSource _stoppingToken = new();
        protected TcpListener Listener { get; }

        private readonly Gauge _openConnections = Metrics.CreateGauge("open_connections", "Currently open connections");
        private ServiceProvider _serverLifetimeProvider;
        protected IServiceCollection Services { get; }

        public int Port { get; }

        public ServerBase(IPacketManager packetManager, ILogger logger, int port, string bindIp = "0.0.0.0")
        {
            _logger = logger;
            PacketManager = packetManager;
            Port = port;
            
            // Start server timer
            _serverTimer.Start();
            
            var localAddr = IPAddress.Parse(bindIp);
            Listener = new TcpListener(localAddr, Port);

            _logger.LogInformation("Initialize tcp server listening on {IP}:{Port}", bindIp, port);

            // Register Core Features
            PacketManager.RegisterNamespace("QuantumCore.Core.Packets");
            RegisterListener<GCHandshake>((connection, packet) => connection.HandleHandshake(packet));
            Services = new ServiceCollection().AddCoreServices()
                .Replace(new ServiceDescriptor(typeof(IPacketManager), _ => packetManager, ServiceLifetime.Singleton));
        }

        public long ServerTime => _serverTimer.ElapsedMilliseconds;

        internal void RemoveConnection(Connection connection)
        {
            _openConnections.Dec();
            _connections.Remove(connection.Id, out _);
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
            
            await using var scope = _serverLifetimeProvider.CreateAsyncScope();
            // cannot inject tcp client here
            var connection = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, client);
            _connections.TryAdd(connection.Id, connection);
                    
            _openConnections.Inc();

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

        public void RegisterListener<P>(Func<T, P, Task> listener)
        {
            _logger.LogDebug("Register listener on packet {TypeName}", typeof(P).Name);
            var packet = PacketManager.IncomingPackets.First(p => p.Value.Type == typeof(P));
            _listeners[packet.Key] = listener;
        }

        public void RegisterNewConnectionListener(Func<Connection, Task<bool>> listener)
        {
            _connectionListeners.Add(listener);
        }

        public void CallListener(Connection connection, object packet)
        {
            var header = PacketManager.IncomingPackets.First(p => p.Value.Type == packet.GetType());
            if (!_listeners.ContainsKey(header.Key)) return;

            var del = _listeners[header.Key];
            del.DynamicInvoke(connection, packet);
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

        public void RegisterListeners<T>() where T : Connection
        {
            var connectionType = typeof(T);
            var listeners = new List<MethodInfo>();
            
            foreach (var method in connectionType.GetMethods())
            {
                var attribute = method.GetCustomAttribute<ListenerAttribute>();
                if (attribute != null)
                {
                    listeners.Add(method);
                }
            }
            foreach (var method in connectionType.GetExtensionMethods())
            {
                var attribute = method.GetCustomAttribute<ListenerAttribute>();
                if (attribute != null)
                {
                    listeners.Add(method);
                }
            }

            foreach (var method in listeners)
            {
                var attribute = method.GetCustomAttribute<ListenerAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                
                _logger.LogDebug("Register listener on packet {PacketName}", attribute.Packet.Name);
                var packet = PacketManager.IncomingPackets.First(p => p.Value.Type == attribute.Packet);

                if (method.IsStatic)
                {
                    _listeners[packet.Key] = (T connection, object p) =>
                    {
                        method.Invoke(null, new[] {connection, p});
                    };
                }
                else
                {
                    _listeners[packet.Key] = (T connection, object p) =>
                    {
                        method.Invoke(connection, new[] {p});
                    };
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