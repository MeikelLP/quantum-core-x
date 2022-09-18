using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Prometheus;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using Serilog;

namespace QuantumCore.Core.Networking
{
    public class Server<T> : IPacketManager where T : Connection
    {
        private readonly int _port;
        private readonly List<Func<T, bool>> _connectionListeners = new();
        private readonly Dictionary<Guid, T> _connections = new();
        private readonly Dictionary<ushort, PacketCache> _incomingPackets = new();
        private readonly List<Type> _incomingTypes = new();
        private readonly Dictionary<ushort, Delegate> _listeners = new();
        private readonly Dictionary<ushort, PacketCache> _outgoingPackets = new();
        private readonly List<Type> _outgoingTypes = new();
        private readonly Stopwatch _serverTimer = new();
        private readonly TcpListener _listener;

        private readonly Gauge _openConnections = Metrics.CreateGauge("open_connections", "Currently open connections");

        private readonly Func<Server<T>, TcpClient, T> _clientConstructor;

        public int Port {
            get {
                return _port;
            }
        }
        
        public Server(Func<Server<T>, TcpClient, T> clientConstructor, int port, string bindIp = "0.0.0.0")
        {
            _port = port;
            
            Log.Information($"Initialize tcp server listening on {bindIp}:{port}");
            _clientConstructor = clientConstructor;
            
            // Start server timer
            _serverTimer.Start();

            var localAddr = IPAddress.Parse(bindIp);
            _listener = new TcpListener(localAddr, port);

            // Register Core Features
            RegisterNamespace("QuantumCore.Core.Packets");
            RegisterListener<GCHandshake>((connection, packet) => connection.HandleHandshake(packet));
        }

        public long ServerTime => _serverTimer.ElapsedMilliseconds;

        internal void RemoveConnection(Connection connection)
        {
            _openConnections.Dec();
            _connections.Remove(connection.Id);
        }

        public Task Start()
        {
            Log.Information("Start listening for connections...");

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientAccepted, _listener);

            return Task.CompletedTask;
        }

        private async void OnClientAccepted(IAsyncResult ar)
        {
            var listener = (TcpListener) ar.AsyncState;
            var client = listener!.EndAcceptTcpClient(ar);
            var connection = _clientConstructor(this, client);
            _connections.Add(connection.Id, connection);
                    
            _openConnections.Inc();

            // wait for new client connection
            _listener.BeginAcceptTcpClient(OnClientAccepted, _listener);
            
            // TODO no while(true)
            await connection.Start();
        }

        public void ForAllConnections(Action<T> callback)
        {
            foreach (var connection in _connections.Values)
            {
                callback(connection);
            }
        }

        public void RegisterListeners()
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
                
                Log.Debug($"Register listener on packet {attribute.Packet.Name}");
                var packet = _incomingPackets.First(p => p.Value.Type == attribute.Packet);

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

        public void RegisterListener<P>(Action<T, P> listener)
        {
            Log.Debug($"Register listener on packet {typeof(P).Name}");
            var packet = _incomingPackets.First(p => p.Value.Type == typeof(P));
            _listeners[packet.Key] = listener;
        }

        public void RegisterNewConnectionListener(Func<Connection, bool> listener)
        {
            _connectionListeners.Add(listener);
        }

        public void CallListener(Connection connection, object packet)
        {
            var header = _incomingPackets.First(p => p.Value.Type == packet.GetType());
            if (!_listeners.ContainsKey(header.Key)) return;

            var del = _listeners[header.Key];
            del.DynamicInvoke(connection, packet);
        }

        public void CallConnectionListener(T connection)
        {
            foreach (var listener in _connectionListeners) listener(connection);
        }

        public void RegisterNamespace(string space, Assembly assembly = null)
        {
            Log.Debug($"Register packet namespace {space}");
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(Server<T>));

            var types = assembly.GetTypes().Where(t => t.Namespace?.StartsWith(space, StringComparison.Ordinal) ?? false)
                .Where(t => t.GetCustomAttribute<PacketAttribute>() != null).ToArray();
            foreach (var type in types)
            {
                Log.Debug($"Register Packet {type.Name}");
                var packet = type.GetCustomAttribute<PacketAttribute>();
                if (packet == null)
                {
                    continue;
                }
                
                var cache = new PacketCache(packet.Header, type);

                var header = (ushort) packet.Header;
                if (cache.IsSubHeader)
                {
                    header = (ushort)(cache.Header << 8 | cache.SubHeader);
                    
                    // We have to create packet cache for the general fields on the first packet for a header which
                    // has a subheader
                    if (packet.Direction.HasFlag(EDirection.Incoming))
                    {
                        if (!_incomingPackets.ContainsKey(header))
                        {
                            _incomingPackets[packet.Header] = cache.CreateGeneralCache();
                        }
                    }

                    if (packet.Direction.HasFlag(EDirection.Outgoing))
                    {
                        if (!_outgoingPackets.ContainsKey(header))
                        {
                            _outgoingPackets[packet.Header] = cache.CreateGeneralCache();
                        }
                    }
                }
                
                if (packet.Direction.HasFlag(EDirection.Incoming))
                {
                    if (_incomingPackets.ContainsKey(header))
                    {
                        Log.Information($"Header 0x{packet.Header} is already in use for incoming packets. ({type.Name} & {_incomingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _incomingPackets.Add(header, cache);
                        _incomingTypes.Add(type);
                    }
                }

                if (packet.Direction.HasFlag(EDirection.Outgoing))
                {
                    if (_outgoingPackets.ContainsKey(header))
                    {
                        Log.Information($"Header 0x{packet.Header} is already in use for outgoing packets. ({type.Name} & {_outgoingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _outgoingPackets.Add(header, cache);
                        _outgoingTypes.Add(type);
                    }
                }
            }
        }

        public bool IsRegisteredOutgoing(Type packet)
        {
            return _outgoingTypes.Contains(packet);
        }

        public PacketCache GetOutgoingPacket(ushort header)
        {
            return !_outgoingPackets.ContainsKey(header) ? null : _outgoingPackets[header];
        }

        public PacketCache GetIncomingPacket(ushort header)
        {
            return !_incomingPackets.ContainsKey(header) ? null : _incomingPackets[header];
        }
    }
}