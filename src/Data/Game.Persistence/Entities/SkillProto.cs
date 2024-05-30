using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game.Persistence.Entities;

public class SkillProto
{
    public required uint Id { get; set; }
    [StringLength(32)] public required string Name { get; set; }
    [DefaultValue(0)] public required short Type { get; set; }
    [DefaultValue(0)] public required short LevelStep { get; set; }
    [DefaultValue(0)] public required short MaxLevel { get; set; }
    [DefaultValue(0)] public required short LevelLimit { get; set; }
    [DefaultValue("0")] [StringLength(100)] public required string PointOn { get; set; } = "0";
    [DefaultValue("")] [StringLength(100)] public required string PointPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string SPCostPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string DurationPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string DurationSPCostPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string CooldownPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string MasterBonusPoly { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string AttackGradePoly { get; set; } = "";
    public required ICollection<ESkillFlag> Flags { get; set; }
    public required ICollection<ESkillAffectFlag> AffectFlags { get; set; } = new List<ESkillAffectFlag>() { ESkillAffectFlag.Ymir };
    [DefaultValue("None")] [StringLength(100)] public required string PointOn2 { get; set; } = "None";
    [DefaultValue("")] [StringLength(100)] public required string PointPoly2 { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string DurationPoly2 { get; set; } = "";
    public required ICollection<ESkillAffectFlag> AffectFlags2 { get; set; } = new List<ESkillAffectFlag>() { ESkillAffectFlag.Ymir };
    [DefaultValue("None")] [StringLength(100)] public required string PointOn3 { get; set; } = "None";
    [DefaultValue("")] [StringLength(100)] public required string PointPoly3 { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string DurationPoly3 { get; set; } = "";
    [DefaultValue("")] [StringLength(100)] public required string GrandMasterAddSPCostPoly { get; set; } = "";
    [DefaultValue(0)] public required int PrerequisiteSkillVnum { get; set; } = 0;
    [DefaultValue(0)] public required int PrerequisiteSkillLevel { get; set; } = 0;
    [DefaultValue(ESkillType.Normal)] public required ESkillType SkillType { get; set; } = ESkillType.Normal;
    [DefaultValue(0)] public required short MaxHit { get; set; } = 0;
    [DefaultValue("1")] [StringLength(100)] public string SplashAroundDamageAdjustPoly { get; set; } = "1";
    [DefaultValue(1000)] public required int TargetRange { get; set; } = 1000;
    [DefaultValue(0)] public required uint SplashRange { get; set; } = 0;

    public static void Configure(EntityTypeBuilder<SkillProto> builder, DatabaseFacade database)
    {
        builder.Property(x => x.Flags).HasConversion(
            v => string.Join(',', v.Select(x => x.ToString())),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<ESkillFlag>).ToList()
        );
        
        builder.Property(x => x.Flags).Metadata.SetValueComparer(
            new ValueComparer<ICollection<ESkillFlag>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        );
        
        builder.Property(x => x.AffectFlags).HasConversion(
            v => string.Join(',', v.Select(x => x.ToString())),
            v => v.Split(',', StringSplitOptions.None).Select(Enum.Parse<ESkillAffectFlag>).ToList()
        );
        
        builder.Property(x => x.AffectFlags).Metadata.SetValueComparer(
            new ValueComparer<ICollection<ESkillAffectFlag>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        );
        
        builder.Property(x => x.AffectFlags2).HasConversion(
            v => string.Join(',', v.Select(x => x.ToString())),
            v => v.Split(',', StringSplitOptions.None).Select(Enum.Parse<ESkillAffectFlag>).ToList()
        );
        
        builder.Property(x => x.AffectFlags2).Metadata.SetValueComparer(
            new ValueComparer<ICollection<ESkillAffectFlag>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        );
        
        builder.Property(x => x.SkillType).HasConversion<string>();

        builder.HasData([
            new SkillProto()
            {
                Id = 1,
                Name = "»ï¿\u00acÂü",
                Type = 1,
                LevelStep = 1,
                MaxLevel = 1,
                LevelLimit = 0,
                PointOn = "HP",
                PointPoly = "-( 1.1*atk + (0.5*atk +  1.5 * str)*k)",
                SPCostPoly = "40+100*k",
                DurationPoly = "",
                DurationSPCostPoly = "",
                CooldownPoly = "12",
                MasterBonusPoly = "-( 1.1*atk + (0.5*atk +  1.5 * str)*k)",
                AttackGradePoly = "",
                Flags = [ESkillFlag.Attack, ESkillFlag.UseMeleeDamage],
                AffectFlags = [ESkillAffectFlag.Ymir],
                PointOn2 = "None",
                PointPoly2 = "",
                DurationPoly2 = "",
                AffectFlags2 = [ESkillAffectFlag.Ymir],
                PointOn3 = "",
                PointPoly3 = "",
                DurationPoly3 = "",
                GrandMasterAddSPCostPoly = "40+100*k",
                PrerequisiteSkillVnum = 0,
                PrerequisiteSkillLevel = 0,
                SkillType = ESkillType.Melee,
                MaxHit = 5,
                SplashAroundDamageAdjustPoly = "1",
                TargetRange = 0,
                SplashRange = 0
            },
            new SkillProto()
            {
                Id = 2,
                Name = "ÆÈ\u00b9æÇ\u00b3¿ì",
                Type = 1,
                LevelStep = 1,
                MaxLevel = 1,
                LevelLimit = 0,
                PointOn = "HP",
                PointPoly = "-(3*atk + (0.8*atk + str*5 + dex*3 +con)*k)",
                SPCostPoly = "50+130*k",
                DurationPoly = "",
                DurationSPCostPoly = "",
                CooldownPoly = "15",
                MasterBonusPoly = "-(3*atk + (0.8*atk + str*5 + dex*3 +con)*k)",
                AttackGradePoly = "",
                Flags = [ESkillFlag.Attack, ESkillFlag.UseMeleeDamage],
                AffectFlags = [ESkillAffectFlag.Ymir],
                PointOn2 = "None",
                PointPoly2 = "",
                DurationPoly2 = "",
                AffectFlags2 = [ESkillAffectFlag.Ymir],
                PointOn3 = "",
                PointPoly3 = "",
                DurationPoly3 = "",
                GrandMasterAddSPCostPoly = "50+130*k",
                PrerequisiteSkillVnum = 0,
                PrerequisiteSkillLevel = 0,
                SkillType = ESkillType.Melee,
                MaxHit = 12,
                SplashAroundDamageAdjustPoly = "1",
                TargetRange = 0,
                SplashRange = 200
            },
            new SkillProto()
            {
                Id = 3,
                Name = "Àü\u00b1ÍÈ\u00a5",
                Type = 1,
                LevelStep = 1,
                MaxLevel = 1,
                LevelLimit = 0,
                PointOn = "ATT_SPEED",
                PointPoly = "50*k",
                SPCostPoly = "50+140*k",
                DurationPoly = "60+90*k",
                DurationSPCostPoly = "",
                CooldownPoly = "63+10*k",
                MasterBonusPoly = "50*k",
                AttackGradePoly = "",
                Flags = [ESkillFlag.SelfOnly],
                AffectFlags = [ESkillAffectFlag.Jeongwihon],
                PointOn2 = "MOV_SPEED",
                PointPoly2 = "20*k",
                DurationPoly2 = "60+90*k",
                AffectFlags2 = [ESkillAffectFlag.Ymir],
                PointOn3 = "",
                PointPoly3 = "",
                DurationPoly3 = "",
                GrandMasterAddSPCostPoly = "50+140*k",
                PrerequisiteSkillVnum = 0,
                PrerequisiteSkillLevel = 0,
                SkillType = ESkillType.Normal,
                MaxHit = 1,
                SplashAroundDamageAdjustPoly = "1",
                TargetRange = 0,
                SplashRange = 0
            },
            new SkillProto()
            {
                Id = 4,
                Name = "\u00b0Ë\u00b0æ",
                Type = 1,
                LevelStep = 1,
                MaxLevel = 1,
                LevelLimit = 0,
                PointOn = "ATT_GRADE",
                PointPoly = "(100 + str + lv * 3)*k",
                SPCostPoly = "100+200*k",
                DurationPoly = "30+50*k",
                DurationSPCostPoly = "",
                CooldownPoly = "30+10*k",
                MasterBonusPoly = "(100 + str + lv * 3)*k",
                AttackGradePoly = "",
                Flags = [ESkillFlag.SelfOnly],
                AffectFlags = [ESkillAffectFlag.Geomgyeong],
                PointOn2 = "NONE",
                PointPoly2 = "",
                DurationPoly2 = "",
                AffectFlags2 = [ESkillAffectFlag.Ymir],
                PointOn3 = "",
                PointPoly3 = "",
                DurationPoly3 = "",
                GrandMasterAddSPCostPoly = "100+200*k",
                PrerequisiteSkillVnum = 0,
                PrerequisiteSkillLevel = 0,
                SkillType = ESkillType.Normal,
                MaxHit = 1,
                SplashAroundDamageAdjustPoly = "1",
                TargetRange = 0,
                SplashRange = 0
            },
            new SkillProto()
            {
                Id = 5,
                Name = "ÅºÈ\u00af\u00b0Ý",
                Type = 1,
                LevelStep = 1,
                MaxLevel = 1,
                LevelLimit = 0,
                PointOn = "HP",
                PointPoly = "-(2*atk + (atk + dex*3 + str*7 + con)*k)",
                SPCostPoly = "60+120*k",
                DurationPoly = "",
                DurationSPCostPoly = "",
                CooldownPoly = "12",
                MasterBonusPoly = "-(2*atk + (atk + dex*3 + str*7 + con)*k)",
                AttackGradePoly = "",
                Flags = [ESkillFlag.Attack, ESkillFlag.UseMeleeDamage, ESkillFlag.Splash, ESkillFlag.Crush],
                AffectFlags = [ESkillAffectFlag.Ymir],
                PointOn2 = "MOV_SPEED",
                PointPoly2 = "150",
                DurationPoly2 = "3",
                AffectFlags2 = [ESkillAffectFlag.Ymir],
                PointOn3 = "",
                PointPoly3 = "",
                DurationPoly3 = "",
                GrandMasterAddSPCostPoly = "60+120*k",
                PrerequisiteSkillVnum = 0,
                PrerequisiteSkillLevel = 0,
                SkillType = ESkillType.Melee,
                MaxHit = 4,
                SplashAroundDamageAdjustPoly = "1",
                TargetRange = 0,
                SplashRange = 200
            },
        ]);
    }
}
