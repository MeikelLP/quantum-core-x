using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[PacketGenerator]
[Packet(0x4B, EDirection.OUTGOING)]
[SubPacket(0x10, 1)]
public partial class GuildName
{
    [Field(0)] public ushort Size => (ushort) Name.Length;
    [Field(1)] public uint Id { get; set; }
    [Field(2, Length = 12)] public string Name { get; set; } = "";
}
