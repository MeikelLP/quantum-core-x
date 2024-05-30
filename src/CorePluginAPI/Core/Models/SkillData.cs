using System.Diagnostics;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.API.Core.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class SkillData
{
    public required uint Id { get; set; }
    public required string Name { get; set; }
    public required short Type { get; set; }
    public required short LevelStep { get; set; }
    public required short MaxLevel { get; set; }
    public required short LevelLimit { get; set; }
    public required string PointOn { get; set; } = "0";
    public required string PointPoly { get; set; } = "";
    public required string SPCostPoly { get; set; } = "";
    public required string DurationPoly { get; set; } = "";
    public required string DurationSPCostPoly { get; set; } = "";
    public required string CooldownPoly { get; set; } = "";
    public required string MasterBonusPoly { get; set; } = "";
    public required string AttackGradePoly { get; set; } = "";
    public required ICollection<ESkillFlag> Flags { get; set; }
    public required ICollection<ESkillAffectFlag> AffectFlags { get; set; } = new List<ESkillAffectFlag>() { ESkillAffectFlag.Ymir };
    public required string PointOn2 { get; set; } = "None";
    public required string PointPoly2 { get; set; } = "";
    public required string DurationPoly2 { get; set; } = "";
    public required ICollection<ESkillAffectFlag> AffectFlags2 { get; set; } = new List<ESkillAffectFlag>() { ESkillAffectFlag.Ymir };
    public required string PointOn3 { get; set; } = "None";
    public required string PointPoly3 { get; set; } = "";
    public required string DurationPoly3 { get; set; } = "";
    public required string GrandMasterAddSPCostPoly { get; set; } = "";
    public required int PrerequisiteSkillVnum { get; set; } = 0;
    public required int PrerequisiteSkillLevel { get; set; } = 0;
    public required ESkillType SkillType { get; set; } = ESkillType.Normal;
    public required short MaxHit { get; set; } = 0;
    public string SplashAroundDamageAdjustPoly { get; set; } = "1";
    public required int TargetRange { get; set; } = 1000;
    public required uint SplashRange { get; set; } = 0;
}
