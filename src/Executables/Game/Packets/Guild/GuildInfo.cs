using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x08, 1)]
[PacketGenerator]
public partial class GuildInfo
{
    [Field(0)] public ushort Size => (ushort) Name.Length;
    [Field(1)] public ushort MemberCount { get; set; }
    [Field(2)] public ushort MaxMemberCount { get; set; }
    [Field(3)] public uint GuildId { get; set; }
    [Field(4)] public uint MasterPid { get; set; }
    [Field(5)] public uint Exp { get; set; }
    [Field(6)] public byte Level { get; set; }
    [Field(7, Length = 13)] public string Name { get; set; } = "";
    [Field(8)] public uint Gold { get; set; }
    [Field(9)] public byte HasLand { get; set; }
}