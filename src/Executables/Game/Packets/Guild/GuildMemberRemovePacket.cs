using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x05, 1)]
[PacketGenerator]
public partial class GuildMemberRemovePacket
{
    public ushort Unused { get; set; }
    public uint PlayerId { get; set; }
}
