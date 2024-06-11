using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

[Flags]
public enum ESkillFlag
{
    Attack = 1 << 0,
    UseMeleeDamage = 1 << 1,
    ComputeAttGrade = 1 << 2,
    SelfOnly = 1 << 3,
    UseMagicDamage = 1 << 4,
    UseHpAsCost = 1 << 5,
    ComputeMagicDamage = 1 << 6,
    Splash = 1 << 7,
    GivePenalty = 1 << 8,
    UseArrowDamage = 1 << 9,
    Penetrate = 1 << 10,
    IgnoreTargetRating = 1 << 11,
    AttackSlow = 1 << 12,
    AttackStun = 1 << 13,
    HpAbsorb = 1 << 14,
    SpAbsorb = 1 << 15,
    AttackFireCount = 1 << 16,
    RemoveBadAffect = 1 << 17,
    RemoveGoodAffect  = 1 << 18,
    Crush = 1 << 19,
    AttackPoison = 1 << 20,
    Toggle = 1 << 21,
    DisableByPointUp = 1 << 22,
    CrushLong = 1 << 23,
    None = 0
}
