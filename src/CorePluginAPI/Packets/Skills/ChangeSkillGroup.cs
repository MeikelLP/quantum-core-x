using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Skills;

[Packet(0x70, EDirection.OUTGOING)]
[PacketGenerator]
public partial class ChangeSkillGroup
{
    [Field(0)] public ESkillGroup SkillGroup { get; set; }
}
