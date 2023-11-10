using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class PacketOperationListener : IPacketOperationListener
{
    private static Histogram _packetsReceived = Metrics.CreateHistogram("packets_received_bytes", "Received packets in bytes");

    private static Histogram _packetsSent = Metrics.CreateHistogram("packets_sent_bytes", "Sent packets in bytes");

    public Task OnPrePacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task OnPostPacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        _packetsReceived.Observe(bytes.Length);
        
        return Task.CompletedTask;
    }

    public Task OnPrePacketSentAsync<T>(T packet, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task OnPostPacketSentAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        _packetsSent.Observe(bytes.Length);
        
        return Task.CompletedTask;
    }
}