using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

[Flags]
public enum ESkillFlag
{
    [EnumMember(Value = "ATTACK")] Attack = 1 << 0,                             // 1
    [EnumMember(Value = "USE_MELEE_DAMAGE")] UseMeleeDamage = 1 << 1,           // 2
    [EnumMember(Value = "COMPUTE_ATTGRADE")] ComputeAttackGrade = 1 << 2,       // 4
    [EnumMember(Value = "SELFONLY")] SelfOnly = 1 << 3,                         // 8
    [EnumMember(Value = "USE_MAGIC_DAMAGE")] UseMagicDamage = 1 << 4,           // 16
    [EnumMember(Value = "USE_HP_AS_COST")] UseHpAsCost = 1 << 5,                // 32
    [EnumMember(Value = "COMPUTE_MAGIC_DAMAGE")] ComputeMagicDamage = 1 << 6,   // 64
    [EnumMember(Value = "SPLASH")] Splash = 1 << 7,                             // 128
    [EnumMember(Value = "GIVE_PENALTY")] GivePenalty = 1 << 8,                  // 256
    [EnumMember(Value = "USE_ARROW_DAMAGE")] UseArrowDamage = 1 << 9,           // 512
    [EnumMember(Value = "PENETRATE")] Penetrate = 1 << 10,                      // 1024
    [EnumMember(Value = "IGNORE_TARGET_RATING")] IgnoreTargetRating = 1 << 11,  // 2048
    [EnumMember(Value = "ATTACK_SLOW")] AttackSlow = 1 << 12,                   // 4096
    [EnumMember(Value = "ATTACK_STUN")] AttackStun = 1 << 13,                   // 8192
    [EnumMember(Value = "HP_ABSORB")] HpAbsorb = 1 << 14,                       // 16384
    [EnumMember(Value = "SP_ABSORB")] SpAbsorb = 1 << 15,                       // 32768
    [EnumMember(Value = "ATTACK_FIRE_CONT")] AttackFireCount = 1 << 16,         // 65536
    [EnumMember(Value = "REMOVE_BAD_AFFECT")] RemoveBadAffect = 1 << 17,        // 131072
    [EnumMember(Value = "REMOVE_GOOD_AFFECT")] RemoveGoodAffect  = 1 << 18,     // 262144
    [EnumMember(Value = "CRUSH")] Crush = 1 << 19,                              // 524288
    [EnumMember(Value = "ATTACK_POISON")] AttackPoison = 1 << 20,               // 1048576
    [EnumMember(Value = "TOGGLE")] Toggle = 1 << 21,                            // 2097152
    [EnumMember(Value = "DISABLE_BY_POINT_UP")] DisableByPointUp = 1 << 22,     // 4194304
    [EnumMember(Value = "CRUSH_LONG")] CrushLong = 1 << 23,                     // 8388608
    [EnumMember(Value = "NONE")] None = 0
}
