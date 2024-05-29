using QuantumCore.API.Game.Skills;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Skills;

public partial class PlayerSkill : IPlayerSkill
{
    [Field(0)]
    public ESkillMasterType MasterType { get; set; }
    [Field(1)]
    public byte Level { get; set; }
    [Field(2)]
    public long NextReadTime { get; set; }
}
