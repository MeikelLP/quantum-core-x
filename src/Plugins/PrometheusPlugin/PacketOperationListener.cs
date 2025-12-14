using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class PacketOperationListener : IPacketOperationListener
{
    private static Histogram PacketsReceived = Metrics.CreateHistogram("packets_received_bytes", "Received packets in bytes");

    private static Histogram PacketsSent = Metrics.CreateHistogram("packets_sent_bytes", "Sent packets in bytes");

    public Task OnPrePacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task OnPostPacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        PacketsReceived.Observe(bytes.Length);
        
        return Task.CompletedTask;
    }

    public Task OnPrePacketSentAsync<T>(T packet, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task OnPostPacketSentAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token)
    {
        PacketsSent.Observe(bytes.Length);
        
        return Task.CompletedTask;
    }
}