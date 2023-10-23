﻿namespace QuantumCore.API.Game.Types;

public enum EApplyType : byte
{
    None = 0,
    MaxHp = 1,
    MaxSp = 2,
    Vitality = 3,
    Intelligence = 4,
    Strength = 5,
    Dexterity = 6,
    AttackSpeed = 7,
    MovementSpeed = 8,
    CastSpeed = 9,
    HpRegen = 10,
    SpRegen = 11,
    PoisonPercentage = 12,
    StunPercentage = 13,
    SlowPercentage = 14,
    CriticalPercentage = 15,
    PenetratePercentage = 16,
    AttackBonusAgainstHuman = 17,
    AttackBonusAgainstAnimal = 18,
    AttackBonusAgainstOrg = 19,
    AttackBonusAgainstMystics = 20,
    AttackBonusAgainstUndead = 21,
    AttackBonusAgainstDevil = 22,
    StealHp = 23,
    StealSp = 24,
    ManaBurnPercentage = 25,
    DamageSpRecover = 26,
    Block = 27,
    Dodge = 28,
    DefenseAgainstSword = 29,
    DefenseAgainstTwohand = 30,
    DefenseAgainstDagger = 31,
    DefenseAgainstBell = 32,
    DefenseAgainstFan = 33,
    DefenseAgainstBow = 34,
    DefenseAgainstFire = 35,
    DefenseAgainstElectric = 36,
    DefenseAgainstMagic = 37,
    DefenseAgainstWind = 38,
    ReflectMelee = 39,
    ReflectCurse = 40,
    PoisonReduce = 41,
    KillSpRecover = 42,
    ExpDoubleBonus = 43,
    GoldDoubleBonus = 44,
    ItemDropBonus = 45,
    PotionBonus = 46,
    KillHpRecover = 47,
    ImmuneStun = 48,
    ImmuneSlow = 49,
    ImmuneFall = 50,
    Skill = 51,
    BowDistance = 52,
    AttackGradeBonus = 53,
    DefenseGradeBonus = 54,
    MagicAttGrade = 55,
    MagicDefGrade = 56,
    CursePercentage = 57,
    MaxStamina = 58,
    AttackBonusAgainstWarrior = 59,
    AttackBonusAgainstAssassin = 60,
    AttackBonusAgainstSura = 61,
    AttackBonusAgainstShaman = 62,
    AttackBonusAgainstMonster = 63,
    NegativeAttackBonus = 64,
    NegativeDefenseBonus = 65,
    NegativeExperienceBonus = 66,
    NegativeItemBonus = 67,
    NegativeGoldBonus = 68,
    MaxHpPercentage = 69,
    MaxSpPercentage = 70,
    SkillDamageBonus = 71,
    NormalHitDamageBonus = 72,
    DefenceAgainstSkillBonus = 73,
    DefenceAgainstNormalHitBonus = 74,
    PcBangExpBonus = 75,
    PcBangDropBonus = 76,
    ExtractHpPercentage = 77,
    DefenseBonusAgainstWarrior = 78,
    DefenseBonusAgainstAssassin = 79,
    DefenseBonusAgainstSura = 80,
    DefenseBonusAgainstShaman = 81,
    Energy = 82,
    DefenseGrade = 83,
    CostumeAttrBonus = 84,
    MagicAttackBonusPercentage = 85,
    AttackBonusAgainstMeleeMagicPercentage = 86,
    DefenseBonusAgainstIce = 87,
    DefenseBonusAgainstEarth = 88,
    DefenseBonusAgainstDark = 89,
    DefenseBonusAgainstCriticalPercentage = 90,
    DefenseBonusAgainstPenetratePercentage = 91
}
