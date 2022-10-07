using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Core.Networking
{
    public abstract class Connection : BackgroundService, IConnection
    {
        private readonly ILogger _logger;
        private readonly PluginExecutor _pluginExecutor;
        private TcpClient _client;

        private IPacketManager _packetManager;

        private Stream _stream;

        private long _lastHandshakeTime;

        public Guid Id { get; }
        public uint Handshake { get; private set; }
        public bool Handshaking { get; private set; }
        public EPhases Phase { get; set; }

        protected Connection(ILogger logger, PluginExecutor pluginExecutor, IPacketManager packetManager)
        {
            _logger = logger;
            _pluginExecutor = pluginExecutor;
            _packetManager = packetManager;
            Id = Guid.NewGuid();
        }

        public void Init(TcpClient client)
        {
            _client = client;
        }

        protected abstract void OnHandshakeFinished();

        protected abstract Task OnClose();

        protected abstract Task OnReceive(object packet);

        protected abstract long GetServerTime();

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("New connection from {RemoteEndPoint}", _client.Client.RemoteEndPoint?.ToString());

            _stream = _client.GetStream();

            await StartHandshake();

            var buffer = new byte[1];
            var packetTotalSize = 1;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var read = await _stream.ReadAsync(buffer.AsMemory(0, 1), stoppingToken);
                    if (read != 1)
                    {
                        _logger.LogInformation("Failed to read, closing connection");
                        _client.Close();
                        break;
                    }

                    packetTotalSize = 1;
                    
                    var packetDetails = _packetManager.GetIncomingPacket(buffer[0]);
                    if (packetDetails == null)
                    {
                        _logger.LogInformation("Received unknown header 0x{Header:X2}", buffer[0]);
                        _client.Close();
                        break;
                    }

                    var data = new byte[packetDetails.Size - 1];
                    read = await _stream.ReadAsync(data, stoppingToken);

                    packetTotalSize += read;
                    var allData = new byte[packetTotalSize];
                    allData[0] = buffer[0];
                    data.CopyTo(allData, 1);

                    if (read != data.Length)
                    {
                        _logger.LogInformation("Failed to read, closing connection");
                        _client.Close();
                        break;
                    }

                    var packet = Activator.CreateInstance(packetDetails.Type);
                    var subHeader = packetDetails.Deserialize(packet, data);

                    if (packetDetails.IsSubHeader)
                    {
                        packetDetails =
                            _packetManager.GetIncomingPacket((ushort) (packetDetails.Header << 8 | subHeader));
                        if (packetDetails == null)
                        {
                            _logger.LogInformation("Received unknown sub header 0x{SubHeader:X2} for header 0x{Header:X2}", subHeader, buffer[0]);
                            _client.Close();
                            break;
                        }

                        packet = Activator.CreateInstance(packetDetails.Type);

                        var subData = new byte[packetDetails.Size - data.Length - 1];
                        read = await _stream.ReadAsync(subData, stoppingToken);
                        
                        packetTotalSize += read;
                        var oldSize = data.Length;
                        Array.Resize(ref data, data.Length + read);
                        data.CopyTo(allData, oldSize);

                        packetDetails.Deserialize(packet, data.Concat(subData).ToArray());
                    }
                    
                    // Check if packet has dynamic data
                    if (packetDetails.IsDynamic)
                    {
                        // Calculate dynamic size
                        var size = packetDetails.GetDynamicSize(packet) - (int)packetDetails.Size;
                        
                        // Read dynamic data
                        var dynamicData = new byte[size];
                        read = await _stream.ReadAsync(dynamicData.AsMemory(0, size), stoppingToken);
                        packetTotalSize += read;
                        var oldSize = data.Length;
                        Array.Resize(ref allData, data.Length + read);
                        dynamicData.CopyTo(allData, oldSize);
                        if (read != size)
                        {
                            _logger.LogInformation("Failed to read dynamic data read {Read} but expected {Size}", read, size);
                            _client.Close();
                            break;
                        }
                        
                        // Copy and deserialize dynamic data into the packet object
                        packetDetails.DeserializeDynamic(packet, dynamicData);
                    }
                    
                    // Check if packet has a sequence
                    if (packetDetails.HasSequence)
                    {
                        var sequence = new byte[1];
                        read = await _stream.ReadAsync(sequence.AsMemory(0, 1), stoppingToken);
                        packetTotalSize += read;
                        var oldSize = data.Length;
                        Array.Resize(ref allData, data.Length + read);
                        sequence.CopyTo(allData, oldSize);
                        if (read != 1)
                        {
                            _client.Close();
                            break;
                        }
                        //_logger.LogDebug($"Read sequence {sequence[0]:X2}");
                    }
                    
                    //_logger.LogDebug($"Recv {packet}");
                    await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPrePacketReceivedAsync(packet, allData, stoppingToken));

                    await OnReceive(packet);

                    await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPostPacketReceivedAsync(packet, allData, stoppingToken));
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Failed to process network data");
                    _client.Close();
                    break;
                }
            }

            Close();
        }

        public void Close()
        {
            _client.Close();
            OnClose();
        }

        public async Task Send(object packet)
        {
            if (!_client.Connected)
            {
                _logger.LogWarning("Tried to send data to a closed connection");
                return;
            }
            
            // Verify that the packet is a packet and registered
            var attr = packet.GetType().GetCustomAttribute<PacketAttribute>();
            if (attr == null) throw new ArgumentException("Given packet is not a packet", nameof(packet));

            if (!_packetManager.IsRegisteredOutgoing(packet.GetType()))
                throw new ArgumentException("Given packet is not a registered outgoing packet", nameof(packet));

            var packetDetails = _packetManager.GetOutgoingPacket(attr.Header);
            
            // Check if packet have sub packets
            if (packetDetails.IsSubHeader)
            {
                var subAttr = packet.GetType().GetCustomAttribute<SubPacketAttribute>();
                Debug.Assert(subAttr != null);
                packetDetails = _packetManager.GetOutgoingPacket((ushort) (attr.Header << 8 | subAttr.SubHeader));
            }
            
            // Check if packet has dynamic data
            if (packetDetails.IsDynamic)
            {
                packetDetails.UpdateDynamicSize(packet, packetDetails.Size);
            }

            // TODO token
            await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPrePacketSentAsync(packet, CancellationToken.None));
            // Serialize object
            var data = packetDetails.Serialize(packet);
            await _stream.WriteAsync(data);
            await _stream.FlushAsync();
            // TODO token
            await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPostPacketReceivedAsync(packet, data, CancellationToken.None));
        }

        public async Task StartHandshake()
        {
            if (Handshaking) return;

            // Generate random handshake and start the handshaking
            Handshake = CoreRandom.GenerateUInt32();
            Handshaking = true;
            await this.SetPhaseAsync(EPhases.Handshake);
            await SendHandshake();
        }

        public async Task<bool> HandleHandshake(GCHandshakeData handshake)
        {
            if (!Handshaking)
            {
                // We wasn't handshaking!
                _logger.LogInformation("Received handshake while not handshaking!");
                _client.Close();
                return false;
            }

            if (handshake.Handshake != Handshake)
            {
                // We received a wrong handshake
                _logger.LogInformation("Received wrong handshake ({Handshake} != {HandshakeHandshake})", Handshake, handshake.Handshake);
                _client.Close();
                return false;
            }

            var time = GetServerTime();
            var difference = time - (handshake.Time + handshake.Delta);
            if (difference >= 0 && difference <= 50)
            {
                // if we difference is less than or equal to 50ms the handshake is done and client time is synced enough
                _logger.LogInformation("Handshake done");
                Handshaking = false;

                OnHandshakeFinished();
            }
            else
            {
                // calculate new delta 
                var delta = (time - handshake.Time) / 2;
                if (delta < 0)
                {
                    delta = (time - _lastHandshakeTime) / 2;
                    _logger.LogDebug($"Delta is too low, retry with last send time");
                }

                await SendHandshake((uint) time, (uint) delta);
            }

            return true;
        }

        private async Task SendHandshake()
        {
            var time = GetServerTime();
            _lastHandshakeTime = time;
            await Send(new GCHandshake
            {
                Handshake = Handshake,
                Time = (uint) time
            });
        }

        private async Task SendHandshake(uint time, uint delta)
        {
            _lastHandshakeTime = time;
            await Send(new GCHandshake
            {
                Handshake = Handshake,
                Time = time,
                Delta = delta
            });
        }
    }
}