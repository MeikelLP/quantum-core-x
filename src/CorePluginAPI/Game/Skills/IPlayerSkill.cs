namespace QuantumCore.API.Game.Skills;

public interface IPlayerSkill
{
    public ESkillMasterType MasterType { get; set; }
    public byte Level { get; set; }
    public long NextReadTime { get; set; }
}
