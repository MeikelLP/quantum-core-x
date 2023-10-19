using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Core.Networking
{
    public abstract class Connection : BackgroundService, IConnection
    {
        private readonly ILogger _logger;
        private readonly PluginExecutor _pluginExecutor;
        private TcpClient _client;
        private readonly ConcurrentQueue<object> _packetsToSend = new();

        private readonly IPacketReader _packetReader;

        private Stream _stream;

        private long _lastHandshakeTime;
        private CancellationTokenSource _cts;

        public Guid Id { get; }
        public uint Handshake { get; private set; }
        public bool Handshaking { get; private set; }
        public EPhases Phase { get; set; }

        protected Connection(ILogger logger, PluginExecutor pluginExecutor, IPacketReader packetReader)
        {
            _logger = logger;
            _pluginExecutor = pluginExecutor;
            _packetReader = packetReader;
            Id = Guid.NewGuid();
        }

        public void Init(TcpClient client)
        {
            _client = client;
            _cts = new CancellationTokenSource();
            SendPacketsWhenPossible().GetAwaiter(); // ignore warning
        }

        protected abstract void OnHandshakeFinished();

        protected abstract Task OnClose();

        protected abstract Task OnReceive(IPacketSerializable packet);

        protected abstract long GetServerTime();

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("New connection from {RemoteEndPoint}", _client.Client.RemoteEndPoint?.ToString());

            _stream = _client.GetStream();
            await StartHandshake();

            try
            {
                await foreach (var packet in _packetReader.EnumerateAsync(_stream, stoppingToken))
                {
                    _logger.LogDebug(" IN: {Type} {Data}", packet.GetType(), JsonSerializer.Serialize(packet));
                    await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPrePacketReceivedAsync(packet, Array.Empty<byte>(), stoppingToken));

                    await OnReceive((IPacketSerializable)packet);

                    await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPostPacketReceivedAsync(packet, Array.Empty<byte>(), stoppingToken));
                }
            }
            catch (IOException e)
            {
                _logger.LogDebug(e, "Connection was closed. Probably by the other party");
                Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read from stream");
                Close();
            }

            Close();
        }

        public void Close()
        {
            _cts.Cancel();
            _client.Close();
            OnClose();
        }

        public void Send<T>(T packet) where T : IPacketSerializable
        {
            _packetsToSend.Enqueue(packet);
        }

        public async Task SendPacketsWhenPossible()
        {
            if (!_client.Connected)
            {
                _logger.LogWarning("Tried to send data to a closed connection");
                return;
            }
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (_packetsToSend.TryDequeue(out var obj))
                    {
                        var packet = (IPacketSerializable) obj;
                        var size = packet.GetSize();
                        var bytes = ArrayPool<byte>.Shared.Rent(size);
                        Array.Clear(bytes, 0, size);
                        packet.Serialize(bytes);
                        var bytesToSend = bytes.AsMemory(0, size);

                        try
                        {
                            await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPrePacketSentAsync(obj, CancellationToken.None)).ConfigureAwait(false);
                            await _stream.WriteAsync(bytesToSend).ConfigureAwait(false);
                            await _stream.FlushAsync().ConfigureAwait(false);
                            await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger, x => x.OnPostPacketReceivedAsync(obj, bytes, CancellationToken.None)).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to send packet");
                        }

                        ArrayPool<byte>.Shared.Return(bytes);
                        _logger.LogInformation("OUT: {Type} => {Packet}", packet.GetType(), JsonSerializer.Serialize(obj));
                    }
                    else
                    {
                        await Task.Delay(1).ConfigureAwait(false); // wait at least 1ms
                    }
                }
                catch (SocketException)
                {
                    // connection closed. Ignore
                    break;
                }
            }
        }

        public async Task StartHandshake()
        {
            if (Handshaking)
            {
                _logger.LogDebug("Already handshaking");
                return;
            }

            // Generate random handshake and start the handshaking
            Handshake = CoreRandom.GenerateUInt32();
            Handshaking = true;
            this.SetPhaseAsync(EPhases.Handshake);
            SendHandshake();
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

                SendHandshake((uint) time, (uint) delta);
            }

            return true;
        }

        private void SendHandshake()
        {
            var time = GetServerTime();
            _lastHandshakeTime = time;
            Send(new GCHandshake(Handshake, (uint)time, 0));
        }

        private void SendHandshake(uint time, uint delta)
        {
            _lastHandshakeTime = time;
            Send(new GCHandshake(Handshake, time, delta));
        }
    }
}
