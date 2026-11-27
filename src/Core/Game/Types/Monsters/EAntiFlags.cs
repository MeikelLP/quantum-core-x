namespace QuantumCore.API.Game.Types.Monsters;

[Flags]
public enum EAntiFlags
{
    FEMALE = (1 << 0),
    MALE = (1 << 1),
    WARRIOR = (1 << 2),
    ASSASSIN = (1 << 3),
    SURA = (1 << 4),
    SHAMAN = (1 << 5),
    DROP = (1 << 7),
    SELL = (1 << 8),
    SAFEBOX = (1 << 17),
}
