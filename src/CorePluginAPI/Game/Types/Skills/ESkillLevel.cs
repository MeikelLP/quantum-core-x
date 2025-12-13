namespace QuantumCore.API.Game.Types.Skills;

public enum ESkillLevel : byte
{
    Unlearned = 0,

    Normal01 = 1,
    Normal02 = 2,
    Normal03 = 3,
    Normal04 = 4,
    Normal05 = 5,
    Normal06 = 6,
    Normal07 = 7,
    Normal08 = 8,
    Normal09 = 9,
    Normal10 = 10,
    Normal11 = 11,
    Normal12 = 12,
    Normal13 = 13,
    Normal14 = 14,
    Normal15 = 15,
    Normal16 = 16,
    Normal17 = 17,
    Normal18 = 18,
    Normal19 = 19,

    MasterM1 = 20,
    MasterM2 = 21,
    MasterM3 = 22,
    MasterM4 = 23,
    MasterM5 = 24,
    MasterM6 = 25,
    MasterM7 = 26,
    MasterM8 = 27,
    MasterM9 = 28,
    MasterM10 = 29,

    GrandMasterG1 = 30,
    GrandMasterG2 = 31,
    GrandMasterG3 = 32,
    GrandMasterG4 = 33,
    GrandMasterG5 = 34,
    GrandMasterG6 = 35,
    GrandMasterG7 = 36,
    GrandMasterG8 = 37,
    GrandMasterG9 = 38,
    GrandMasterG10 = 39,

    PerfectMasterP = 40
}

public static class ESkillLevelUtils
{
    public static ESkillLevel Min(ESkillLevel a, ESkillLevel b) => a < b ? a : b;
}
