namespace QuantumCore.API.PluginTypes;

public interface IPacketOperationListener
{
    Task OnPrePacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token);
    Task OnPostPacketReceivedAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token);
    Task OnPostPacketSentAsync<T>(T packet, ReadOnlySpan<byte> bytes, CancellationToken token);
    Task OnPrePacketSentAsync<T>(T packet, CancellationToken token);
}