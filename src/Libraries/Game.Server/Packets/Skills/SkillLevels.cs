using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Skills;

[Packet(0x4C, EDirection.Outgoing)]
[PacketGenerator]
public partial class SkillLevels
{
    [Field(0, ArrayLength = 255)] public PlayerSkill[] Skills { get; set; } = new PlayerSkill[255];
}