using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing, Sequence = true)]
[SubPacket(0x04, 1)]
[PacketGenerator]
public partial class GuildMemberAddPacket
{
    public byte Unused { get; set; }
    public uint PlayerId { get; set; }
}