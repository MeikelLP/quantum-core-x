namespace QuantumCore.API.Extensions;

public static class PlayerExtensions
{
    public static EPlayerClass GetClass(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WarriorMale => EPlayerClass.Warrior,
            EPlayerClassGendered.NinjaFemale => EPlayerClass.Ninja,
            EPlayerClassGendered.SuraMale => EPlayerClass.Sura,
            EPlayerClassGendered.ShamanFemale => EPlayerClass.Shaman,
            EPlayerClassGendered.WarriorFemale => EPlayerClass.Warrior,
            EPlayerClassGendered.NinjaMale => EPlayerClass.Ninja,
            EPlayerClassGendered.SuraFemale => EPlayerClass.Sura,
            EPlayerClassGendered.ShamanMale => EPlayerClass.Shaman,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }

    public static EPlayerGender GetGender(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WarriorMale => EPlayerGender.Male,
            EPlayerClassGendered.NinjaFemale => EPlayerGender.Female,
            EPlayerClassGendered.SuraMale => EPlayerGender.Male,
            EPlayerClassGendered.ShamanFemale => EPlayerGender.Female,
            EPlayerClassGendered.WarriorFemale => EPlayerGender.Female,
            EPlayerClassGendered.NinjaMale => EPlayerGender.Male,
            EPlayerClassGendered.SuraFemale => EPlayerGender.Female,
            EPlayerClassGendered.ShamanMale => EPlayerGender.Male,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }
}
