using System.Diagnostics;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.API.Core.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class SkillData
{
    public required uint Id { get; set; }
    public string Name { get; set; }
    public ESkillCategoryType Type { get; set; }
    public short LevelStep { get; set; }
    public short MaxLevel { get; set; }
    public short LevelLimit { get; set; }
    public string PointOn { get; set; } = "0";
    public string PointPoly { get; set; } = "";
    public string SPCostPoly { get; set; } = "";
    public string DurationPoly { get; set; } = "";
    public string DurationSPCostPoly { get; set; } = "";
    public string CooldownPoly { get; set; } = "";
    public string MasterBonusPoly { get; set; } = "";
    public string AttackGradePoly { get; set; } = "";
    public ESkillFlag Flag { get; set; }
    public ESkillAffectFlag AffectFlag { get; set; } = ESkillAffectFlag.Ymir;
    public string PointOn2 { get; set; } = "None";
    public string PointPoly2 { get; set; } = "";
    public string DurationPoly2 { get; set; } = "";
    public ESkillAffectFlag AffectFlag2 { get; set; } = ESkillAffectFlag.Ymir;
    public int PrerequisiteSkillVnum { get; set; } = 0;
    public int PrerequisiteSkillLevel { get; set; } = 0;
    public ESkillType SkillType { get; set; } = ESkillType.Normal;
    public short MaxHit { get; set; } = 0;
    public string SplashAroundDamageAdjustPoly { get; set; } = "1";
    public int TargetRange { get; set; } = 1000;
    public uint SplashRange { get; set; } = 0;
}
