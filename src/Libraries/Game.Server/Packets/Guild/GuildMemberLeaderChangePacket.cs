using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.OUTGOING)]
[SubPacket(0xD, 1)]
[PacketGenerator]
public partial class GuildMemberLeaderChangePacket
{
    [Field(0)] public ushort Unused { get; set; }
    [Field(1)] public uint PlayerId { get; set; }
    [Field(2)] public bool IsLeader { get; set; }
}
