using System.Diagnostics.CodeAnalysis;

namespace QuantumCore.Networking;

public interface IPacketReader2
{
    bool TryGetPacket(in PacketHeaderDefinition header, in ReadOnlySpan<byte> buffer,
        [MaybeNullWhen(false)] out IPacket packet);

    ValueTask HandlePacketAsync(IServiceProvider scopedServiceProvider, PacketHeaderDefinition header, IPacket packet,
        CancellationToken token = default);

    /// <summary>
    /// Try to get a packet info by their header value
    /// </summary>
    /// <param name="header"></param>
    /// <param name="packet"></param>
    /// <returns>True if found</returns>
    bool TryGetPacketInfo(in PacketHeaderDefinition header, [MaybeNullWhen(false)] out PacketInfo2 packet);

    /// <summary>
    /// Check if the given header has sub packets defined
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    bool IsSubPacketDefinition(in byte header);
}
