using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.API.Game.Skills;

public interface ISkill
{
    public ESkill SkillId { get; set; }
    public ESkillMasterType MasterType { get; set; }
    public ESkillLevel Level { get; set; }
    public int NextReadTime { get; set; }
}
