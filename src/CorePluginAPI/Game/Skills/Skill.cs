namespace QuantumCore.API.Game.Skills;

public class Skill : ISKill
{
    public uint SkillId { get; set; }
    public uint PlayerId { get; set; }
    public ESkillMasterType MasterType { get; set; }
    public byte Level { get; set; }
    public int NextReadTime { get; set; }
    public uint ReadsRequired { get; set; }
}
