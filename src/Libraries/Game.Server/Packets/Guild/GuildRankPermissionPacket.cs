using QuantumCore.API.Game.Types.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x07, 1)]
[PacketGenerator]
public partial class GuildRankPermissionPacket
{
    [Field(0)] public ushort Size { get; set; }
    [Field(1)] public byte Position { get; set; }
    [Field(2)] public GuildRankPermissions Permissions { get; set; }
}
