using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using QuantumCore.Core.Packets;
using Serilog;

namespace QuantumCore.Core.Networking
{
    public class Server<T> : IPacketManager where T : Connection
    {
        private readonly List<Func<T, bool>> _connectionListeners = new List<Func<T, bool>>();
        private readonly Dictionary<Guid, T> _connections = new Dictionary<Guid, T>();
        private readonly Dictionary<byte, PacketCache> _incomingPackets = new Dictionary<byte, PacketCache>();
        private readonly List<Type> _incomingTypes = new List<Type>();
        private readonly Dictionary<byte, Delegate> _listeners = new Dictionary<byte, Delegate>();
        private readonly Dictionary<byte, PacketCache> _outgoingPackets = new Dictionary<byte, PacketCache>();
        private readonly List<Type> _outgoingTypes = new List<Type>();
        private readonly Stopwatch _serverTimer = new Stopwatch();
        private readonly TcpListener _listener;

        private readonly Func<Server<T>, TcpClient, T> _clientConstructor;

        public Server(Func<Server<T>, TcpClient, T> clientConstructor, int port, string bindIp = "0.0.0.0")
        {
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
            _connections.Remove(connection.Id);
        }

        public async Task Start()
        {
            Log.Information("Start listening for connections...");
            _listener.Start();

            while (true)
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    var connection = _clientConstructor(this, client);
                    _connections.Add(connection.Id, connection);

                    connection.Start();
                } catch(Exception e) {
                    Log.Fatal(e.Message);
                }
            
            Console.WriteLine("HALLO");
        }

        public void RegisterListener<P>(Action<T, P> listener)
        {
            Log.Debug($"Register listener on packet {typeof(P).Name}");
            var packet = _incomingPackets.Where(p => p.Value.Type == typeof(P)).Select(p => p.Value).First();
            _listeners[packet.Header] = listener;
        }

        public void RegisterNewConnectionListener(Func<Connection, bool> listener)
        {
            _connectionListeners.Add(listener);
        }

        public void CallListener(Connection connection, object packet)
        {
            var header = _incomingPackets.Where(p => p.Value.Type == packet.GetType()).Select(p => p.Value.Header)
                .First();
            if (!_listeners.ContainsKey(header)) return;

            var del = _listeners[header];
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

            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, space, StringComparison.Ordinal))
                .Where(t => t.GetCustomAttribute<PacketAttribute>() != null).ToArray();
            foreach (var type in types)
            {
                Log.Debug($"Register Packet {type.Name}");
                var packet = type.GetCustomAttribute<PacketAttribute>();
                if (packet.Direction.HasFlag(EDirection.Incoming))
                {
                    if (_incomingPackets.ContainsKey(packet.Header))
                    {
                        Log.Information($"Header 0x{packet.Header} is already in use for incoming packets. ({type.Name} & {_incomingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _incomingPackets.Add(packet.Header, new PacketCache(packet.Header, type));
                        _incomingTypes.Add(type);
                    }
                }

                if (packet.Direction.HasFlag(EDirection.Outgoing))
                {
                    if (_outgoingPackets.ContainsKey(packet.Header))
                    {
                        Log.Information($"Header 0x{packet.Header} is already in use for outgoing packets. ({type.Name} & {_outgoingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _outgoingPackets.Add(packet.Header, new PacketCache(packet.Header, type));
                        _outgoingTypes.Add(type);
                    }
                }
            }
        }

        public bool IsRegisteredOutgoing(Type packet)
        {
            return _outgoingTypes.Contains(packet);
        }

        public PacketCache GetOutgoingPacket(byte header)
        {
            return !_outgoingPackets.ContainsKey(header) ? null : _outgoingPackets[header];
        }

        public PacketCache GetIncomingPacket(byte header)
        {
            return !_incomingPackets.ContainsKey(header) ? null : _incomingPackets[header];
        }
    }
}