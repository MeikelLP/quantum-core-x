namespace QuantumCore.Networking;

public interface INewPacketManager
{
    bool TryGetPacketInfo(in byte header, in byte? subHeader, out PacketInfo packet);
    bool TryGetPacketInfo(IPacketSerializable packet, out PacketInfo info);
    bool IsSubPacketDefinition(in byte header);
}