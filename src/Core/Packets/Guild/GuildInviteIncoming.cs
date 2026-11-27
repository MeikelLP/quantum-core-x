using QuantumCore.Networking;

namespace QuantumCore.API.Packets.Guild;

[Packet(0x50, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x00, 0)]
[PacketGenerator]
public partial class GuildInviteIncoming
{
    public uint InvitedPlayerId { get; set; }
}
