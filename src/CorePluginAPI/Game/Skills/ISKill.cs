using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.API.Game.Skills;

public interface ISKill
{
    public ESkill SkillId { get; set; }
    public ESkillMasterType MasterType { get; set; }
    public byte Level { get; set; }
    public int NextReadTime { get; set; }
}
