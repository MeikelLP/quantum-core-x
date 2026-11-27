using QuantumCore.Game.Types.Skills;
using QuantumCore.Networking;

namespace QuantumCore.Packets.Skills;

public partial class PlayerSkill
{
    [Field(0)] public ESkillMasterType MasterType { get; set; }
    [Field(1)] public ESkillLevel Level { get; set; }
    [Field(2)] public int NextReadTime { get; set; }
}
