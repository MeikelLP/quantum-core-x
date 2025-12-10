using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x02, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class Attack
    {
        [Field(0)] public ESkill SkillMotion { get; set; }
        [Field(1)] public uint Vid { get; set; }
        [Field(2, ArrayLength = 2)] public byte[] Unknown { get; set; } = new byte[2] {0, 0};
    }
}
