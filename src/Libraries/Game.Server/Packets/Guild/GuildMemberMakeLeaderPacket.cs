using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x0A, 0)]
[PacketGenerator]
public partial class GuildMemberMakeLeaderPacket
{
    [Field(0)] public uint PlayerId { get; set; }
    [Field(1)] public bool IsLeader { get; set; }
}
