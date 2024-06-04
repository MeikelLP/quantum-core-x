namespace QuantumCore.API.Game.Skills;

public interface ISKill
{
    public uint SkillId { get; set; }
    public ESkillMasterType MasterType { get; set; }
    public byte Level { get; set; }
    public int NextReadTime { get; set; }
}
