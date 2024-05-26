using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Core.Networking
{
    public abstract class Connection : BackgroundService, IConnection
    {
        private readonly ILogger _logger;
        private readonly IPluginExecutor _pluginExecutor;
        private readonly ConcurrentQueue<byte[]> _packetsToSend = new();
        private readonly IPacketReader _packetReader;

        private TcpClient? _client;
        private Stream? _stream;
        private long _lastHandshakeTime;
        private CancellationTokenSource? _cts;

        public Guid Id { get; }
        public uint Handshake { get; private set; }
        public bool Handshaking { get; private set; }
        public EPhases Phase { get; set; }

        protected Connection(ILogger logger, IPluginExecutor pluginExecutor, IPacketReader packetReader)
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
            Task.Factory.StartNew(SendPacketsWhenAvailable, TaskCreationOptions.LongRunning);
        }

        protected abstract void OnHandshakeFinished();

        protected abstract Task OnClose(bool expected = true);

        protected abstract long GetServerTime();

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_client is null)
            {
                _logger.LogCritical("Cannot execute when client is null");
                return;
            }

            _logger.LogInformation("New connection from {RemoteEndPoint}", _client.Client.RemoteEndPoint?.ToString());

            _stream = _client.GetStream();
            StartHandshake();

            try
            {
                // await Task.Factory.StartNew(async () =>
                // {
                //     await _packetReader.EnumerateAsync(_stream, stoppingToken);
                // }, TaskCreationOptions.LongRunning);
            }
            catch (IOException e)
            {
                _logger.LogDebug(e, "Connection was closed. Probably by the other party");
                Close(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read from stream");
                Close(false);
            }

            Close(false);
        }

        public void Close(bool expected = true)
        {
            _cts?.Cancel();
            _client?.Close();
            OnClose(expected);
        }

        public void Send(byte[] packet)
        {
            _packetsToSend.Enqueue(packet);
        }

        private async Task SendPacketsWhenAvailable()
        {
            if (_client?.Connected != true)
            {
                _logger.LogWarning("Tried to send data to a closed connection");
                return;
            }

            while (_cts?.IsCancellationRequested != true)
            {
                try
                {
                    if (_packetsToSend.TryDequeue(out var bytes))
                    {
                        try
                        {
                            if (_stream is null)
                            {
                                _cts?.Cancel();
                                _logger.LogCritical("Stream unexpectedly became null. This shouldn't happen");
                                break;
                            }

                            // await _pluginExecutor
                            // .ExecutePlugins<IPacketOperationListener>(_logger,
                            //     x => x.OnPrePacketSentAsync(obj, CancellationToken.None)).ConfigureAwait(false);
                            await _stream.WriteAsync(bytes).ConfigureAwait(false);
                            await _stream.FlushAsync().ConfigureAwait(false);
                            // await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger,
                            //     x => x.OnPostPacketReceivedAsync(obj, bytes, CancellationToken.None))
                            // .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to send packet");
                        }

                        ArrayPool<byte>.Shared.Return(bytes);
                        // _logger.LogDebug("OUT: {Type} => {Packet}", packet.GetType(), JsonSerializer.Serialize(obj));
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

        public void StartHandshake()
        {
            if (Handshaking)
            {
                _logger.LogDebug("Already handshaking");
                return;
            }

            // Generate random handshake and start the handshaking
            Handshake = CoreRandom.GenerateUInt32();
            Handshaking = true;
            this.SetPhase(EPhases.Handshake);
            SendHandshake();
        }

        public bool HandleHandshake(GCHandshakeData handshake)
        {
            if (!Handshaking)
            {
                // We wasn't handshaking!
                _logger.LogInformation("Received handshake while not handshaking!");
                _client?.Close();
                return false;
            }

            if (handshake.Handshake != Handshake)
            {
                // We received a wrong handshake
                _logger.LogInformation("Received wrong handshake ({Handshake} != {HandshakeHandshake})", Handshake,
                    handshake.Handshake);
                _client?.Close();
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
            Send(new GCHandshake {Handshake = Handshake, Time = (uint) time, Delta = 0});
        }

        private void SendHandshake(uint time, uint delta)
        {
            _lastHandshakeTime = time;
            Send(new GCHandshake {Handshake = Handshake, Time = time, Delta = delta});
        }
    }
}