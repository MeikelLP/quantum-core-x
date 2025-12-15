using QuantumCore.API.Game.Types.Players;

namespace QuantumCore.API.Extensions;

public static class PlayerExtensions
{
    public static EPlayerClass GetClass(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WARRIOR_MALE => EPlayerClass.WARRIOR,
            EPlayerClassGendered.NINJA_FEMALE => EPlayerClass.NINJA,
            EPlayerClassGendered.SURA_MALE => EPlayerClass.SURA,
            EPlayerClassGendered.SHAMAN_FEMALE => EPlayerClass.SHAMAN,
            EPlayerClassGendered.WARRIOR_FEMALE => EPlayerClass.WARRIOR,
            EPlayerClassGendered.NINJA_MALE => EPlayerClass.NINJA,
            EPlayerClassGendered.SURA_FEMALE => EPlayerClass.SURA,
            EPlayerClassGendered.SHAMAN_MALE => EPlayerClass.SHAMAN,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }

    public static EPlayerGender GetGender(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WARRIOR_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.NINJA_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.SURA_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.SHAMAN_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.WARRIOR_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.NINJA_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.SURA_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.SHAMAN_MALE => EPlayerGender.MALE,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }
}
