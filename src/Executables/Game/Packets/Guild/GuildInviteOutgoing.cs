using QuantumCore.API.Game.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0xE, 1)]
[PacketGenerator]
public partial class GuildInviteOutgoing
{
    [Field(0)] public ushort Size => (byte) GuildName.Length;

    [Field(1)] public uint GuildId { get; set; }

    [Field(2, Length = GuildConstants.NAME_MAX_LENGTH + 1)]
    public string GuildName { get; set; } = "";
}
