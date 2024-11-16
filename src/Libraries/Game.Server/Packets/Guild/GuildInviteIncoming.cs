using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x00, 0)]
[PacketGenerator]
public partial class GuildInviteIncoming
{
    public uint InvitedPlayerId { get; set; }
}