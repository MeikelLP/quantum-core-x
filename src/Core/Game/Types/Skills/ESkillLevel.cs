namespace QuantumCore.API.Game.Types.Skills;

public enum ESkillLevel : byte
{
    UNLEARNED = 0,

    NORMAL01 = 1,
    NORMAL02 = 2,
    NORMAL03 = 3,
    NORMAL04 = 4,
    NORMAL05 = 5,
    NORMAL06 = 6,
    NORMAL07 = 7,
    NORMAL08 = 8,
    NORMAL09 = 9,
    NORMAL10 = 10,
    NORMAL11 = 11,
    NORMAL12 = 12,
    NORMAL13 = 13,
    NORMAL14 = 14,
    NORMAL15 = 15,
    NORMAL16 = 16,
    NORMAL17 = 17,
    NORMAL18 = 18,
    NORMAL19 = 19,

    MASTER_M1 = 20,
    MASTER_M2 = 21,
    MASTER_M3 = 22,
    MASTER_M4 = 23,
    MASTER_M5 = 24,
    MASTER_M6 = 25,
    MASTER_M7 = 26,
    MASTER_M8 = 27,
    MASTER_M9 = 28,
    MASTER_M10 = 29,

    GRAND_MASTER_G1 = 30,
    GRAND_MASTER_G2 = 31,
    GRAND_MASTER_G3 = 32,
    GRAND_MASTER_G4 = 33,
    GRAND_MASTER_G5 = 34,
    GRAND_MASTER_G6 = 35,
    GRAND_MASTER_G7 = 36,
    GRAND_MASTER_G8 = 37,
    GRAND_MASTER_G9 = 38,
    GRAND_MASTER_G10 = 39,

    PERFECT_MASTER_P = 40
}

public static class ESkillLevelUtils
{
    public static ESkillLevel Min(ESkillLevel a, ESkillLevel b) => a < b ? a : b;
}
