using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x0B, 0)]
[PacketGenerator]
public partial class GuildInviteResponse
{
    [Field(0)] public uint GuildId { get; set; }
    [Field(1)] public bool WantsToJoin { get; set; }
}