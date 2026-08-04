using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x04, 0)]
[PacketGenerator]
public partial class GuildExperienceInvestPacket
{
    [Field(0)] public uint Amount { get; set; }
}
