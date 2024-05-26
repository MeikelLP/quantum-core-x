namespace QuantumCore.Networking;

/// <summary>
/// Stores meta information about packets. This information should be only used for deserialization.
/// </summary>
public interface IPacketManager
{
    /// <summary>
    /// Try to get a packet info by their header value
    /// </summary>
    /// <param name="header"></param>
    /// <param name="subHeader"></param>
    /// <param name="packet"></param>
    /// <returns>True if found</returns>
    bool TryGetPacketInfo(in byte header, in byte? subHeader, out PacketInfo packet);


    /// <summary>
    /// Try to get a packet info by an actual packet
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="info"></param>
    /// <returns>True if found</returns>
    bool TryGetPacketInfo(IPacketSerializable packet, out PacketInfo info);

    /// <summary>
    /// Check if the given header has sub packets defined
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    bool IsSubPacketDefinition(in byte header);
}