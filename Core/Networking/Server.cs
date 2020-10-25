using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using QuantumCore.Core.Packets;

namespace QuantumCore.Core.Networking {
    public class Server {
        private TcpListener _listener;
        private readonly Dictionary<Guid, Connection> _connections = new Dictionary<Guid, Connection>();
        private readonly Dictionary<byte, PacketCache> _incomingPackets = new Dictionary<byte, PacketCache>();
        private readonly List<Type> _incomingTypes = new List<Type>();
        private readonly Dictionary<byte, PacketCache> _outgoingPackets = new Dictionary<byte, PacketCache>();
        private readonly List<Type> _outgoingTypes = new List<Type>();
        private readonly Dictionary<byte, Delegate> _listeners = new Dictionary<byte, Delegate>(); 
        private readonly List<Func<Connection, bool>> _connectionListeners = new List<Func<Connection, bool>>();
        public Random Random { get; private set; } = new Random();
        private readonly Stopwatch _serverTimer = new Stopwatch();
        public long ServerTime {
            get 
            {
                return _serverTimer.ElapsedMilliseconds;
            }
        }

        public Server(int port, string bindIp = "0.0.0.0")
        {
            // Start server timer
            _serverTimer.Start();

            var localAddr = IPAddress.Parse(bindIp);
            _listener = new TcpListener(localAddr, port);

            // Register Core Features
            RegisterNamespace("QuantumCore.Core.Packets");
            RegisterListener<GCHandshake>((connection, packet) => {
                return connection.HandleHandshake(packet);
            });
        }

        internal void RemoveConnection(Connection connection) {
            _connections.Remove(connection.Id);
        }

        public async void Start() {
            _listener.Start();

            while (true) 
            {
                try {
                    var client = await _listener.AcceptTcpClientAsync();
                    var connection = new Connection(client, this);
                    _connections.Add(connection.Id, connection);

                    connection.Start();
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
                
            }
        }

        public void RegisterListener<T>(Func<Connection, T, bool> listener) {
            var packet = _incomingPackets.Where(p => p.Value.Type == typeof(T)).Select(p => p.Value).First();
            _listeners[packet.Header] = listener;
        }

        public void RegisterNewConnectionListener(Func<Connection, bool> listener) {
            _connectionListeners.Add(listener);
        }

        public void CallListener(Connection connection, object packet) {
            var header = _incomingPackets.Where(p => p.Value.Type == packet.GetType()).Select(p => p.Value.Header).First();
            if(_listeners.ContainsKey(header))
            {
                var del = _listeners[header];
                del.DynamicInvoke(connection, packet);
            }
        }

        public void CallConnectionListener(Connection connection) {
            foreach(var listener in _connectionListeners) {
                listener(connection);
            }
        }

        public void RegisterNamespace(string space, Assembly assembly = null) 
        {
            if(assembly == null) 
            {
                assembly = Assembly.GetAssembly(typeof(Server));
            }

            var types = assembly.GetTypes().Where(t => String.Equals(t.Namespace, space, StringComparison.Ordinal)).Where(t => t.GetCustomAttribute<Packet>() != null).ToArray();
            foreach(var type in types) 
            {
                Console.WriteLine($"Register Packet {type.Name}");
                var packet = type.GetCustomAttribute<Packet>();
                if(packet.Direction.HasFlag(EDirection.Incoming)) 
                {
                    if(_incomingPackets.ContainsKey(packet.Header)) 
                    {
                        Console.WriteLine($"Header 0x{packet.Header} is already in use for incoming packets. ({type.Name} & {_incomingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _incomingPackets.Add(packet.Header, new PacketCache(packet.Header, type));
                        _incomingTypes.Add(type);
                    }
                } 
                if(packet.Direction.HasFlag(EDirection.Outgoing))
                {
                    if(_outgoingPackets.ContainsKey(packet.Header)) 
                    {
                        Console.WriteLine($"Header 0x{packet.Header} is already in use for outgoing packets. ({type.Name} & {_outgoingPackets[packet.Header].Type.Name})");
                    }
                    else
                    {
                        _outgoingPackets.Add(packet.Header, new PacketCache(packet.Header, type));
                        _outgoingTypes.Add(type);
                    }
                }
            }
        }

        public bool IsRegisteredOutgoing(Type packet) {
            return _outgoingTypes.Contains(packet);
        }

        public PacketCache GetOutgoingPacket(byte header) {
            if(!_outgoingPackets.ContainsKey(header)) return null;
            return _outgoingPackets[header];
        }

        public PacketCache GetIncomingPacket(byte header) {
            if(!_incomingPackets.ContainsKey(header)) return null;
            return _incomingPackets[header];
        }
    }
}