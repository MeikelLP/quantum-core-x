using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Skills;

[Packet(0x70, EDirection.Outgoing)]
[PacketGenerator]
public partial class ChangeSkillGroup
{
    [Field(0)]
    public byte SkillGroup { get; set; }
}
