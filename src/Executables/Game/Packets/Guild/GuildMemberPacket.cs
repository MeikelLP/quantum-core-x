using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x02, 1)]
[PacketGenerator]
public partial class GuildMemberPacket
{
    [Field(0)] public ushort Size => (ushort) Members.Length;
    [Field(1)] public GuildMember[] Members { get; set; } = [];
}

public class GuildMember
{
    [Field(0)] public uint PlayerId { get; set; }
    [Field(1)] public byte Grade { get; set; }
    [Field(2)] public bool IsGeneral { get; set; }
    [Field(3)] public byte Class { get; set; }
    [Field(4)] public byte Level { get; set; }
    [Field(5)] public uint Offer { get; set; }
    [Field(6)] public bool IsNameSent { get; set; }
    [Field(7, Length = 25)] public string Name { get; set; } = "";
}
