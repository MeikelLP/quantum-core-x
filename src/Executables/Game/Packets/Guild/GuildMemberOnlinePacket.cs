using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x00, 1)]
[PacketGenerator]
public partial class GuildMemberOnlinePacket
{
    public ushort Unused { get; set; }
    public uint PlayerId { get; set; }
}
