using QuantumCore.Networking;

namespace QuantumCore.API.Packets.Skills;

[Packet(0x4C, EDirection.OUTGOING)]
[PacketGenerator]
public partial class SkillLevels
{
    [Field(0, ArrayLength = 255)] public PlayerSkill[] Skills { get; set; } = new PlayerSkill[255];
}
