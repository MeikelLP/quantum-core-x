using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x34, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class PlayerUseSkill
{
    [Field(0)]
    public int SkillId { get; set; }
    [Field(1)]
    public int TargetVid { get; set; }
}
