using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
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

namespace QuantumCore.Core.Networking;

public abstract class Connection : BackgroundService, IConnection
{
    private readonly ILogger _logger;
    private readonly PluginExecutor _pluginExecutor;
    private readonly ConcurrentQueue<object> _packetsToSend = new();
    private readonly IPacketReader _packetReader;

    private TcpClient? _client;
    private Stream? _stream;
    private long _lastHandshakeTime;
    private CancellationTokenSource? _cts;

    public IPAddress BoundIpAddress { get; private set; }

    public Guid Id { get; }
    public uint Handshake { get; private set; }
    public bool Handshaking { get; private set; }
    public EPhase Phase { get; set; }

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
        BoundIpAddress = ((IPEndPoint)_client.Client.LocalEndPoint!).Address;
        _cts = new CancellationTokenSource();
        Task.Factory.StartNew(SendPacketsWhenAvailable, TaskCreationOptions.LongRunning);
    }

    protected abstract void OnHandshakeFinished();

    protected abstract Task OnClose(bool expected = true);

    protected abstract Task OnReceive(IPacketSerializable packet);

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
            await foreach (var packet in _packetReader.EnumerateAsync(_stream, stoppingToken))
            {
                // _logger.LogDebug(" IN: {Type} {Data}", packet.GetType(), JsonSerializer.Serialize(packet));
                await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger,
                    x => x.OnPrePacketReceivedAsync(packet, Array.Empty<byte>(), stoppingToken));

                await OnReceive((IPacketSerializable) packet);

                await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger,
                    x => x.OnPostPacketReceivedAsync(packet, Array.Empty<byte>(), stoppingToken));
            }
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

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
            _stream?.Dispose();
            _cts?.Dispose();
        }
    }

    public override sealed void Dispose()
    {
        Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Send<T>(T packet) where T : IPacketSerializable
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
                        if (_stream is null)
                        {
                            _cts?.Cancel();
                            _logger.LogCritical("Stream unexpectedly became null. This shouldn't happen");
                            break;
                        }

                        await _pluginExecutor
                            .ExecutePlugins<IPacketOperationListener>(_logger,
                                x => x.OnPrePacketSentAsync(obj, CancellationToken.None)).ConfigureAwait(false);
                        await _stream.WriteAsync(bytesToSend).ConfigureAwait(false);
                        await _stream.FlushAsync().ConfigureAwait(false);
                        await _pluginExecutor.ExecutePlugins<IPacketOperationListener>(_logger,
                                x => x.OnPostPacketSentAsync(obj, bytes, CancellationToken.None))
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to send packet");
                    }

                    _logger.LogDebug("OUT: {Type} => {Packet} (0x{Bytes})", packet.GetType(),
                        JsonSerializer.Serialize(obj),
                        string.Join("", bytesToSend.ToArray().Select(x => x.ToString("X2"))));
                    ArrayPool<byte>.Shared.Return(bytes);
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
        this.SetPhase(EPhase.Handshake);
        SendHandshake();
    }

    public bool HandleHandshake(GcHandshakeData handshake)
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
        Send(new GcHandshake(Handshake, (uint) time, 0));
    }

    private void SendHandshake(uint time, uint delta)
    {
        _lastHandshakeTime = time;
        Send(new GcHandshake(Handshake, time, delta));
    }
}
