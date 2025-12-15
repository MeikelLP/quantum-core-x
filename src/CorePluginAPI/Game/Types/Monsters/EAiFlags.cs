namespace QuantumCore.API.Game.Types.Monsters;

[Flags]
public enum EAiFlags
{
    AGGRESSIVE = 1 << 0,
    NO_MOVE = 1 << 1,
    COWARD = 1 << 2,
    NO_ATTACK_SHINSOO = 1 << 3,
    NO_ATTACK_CHUNJO = 1 << 4,
    NO_ATTACK_JINNO = 1 << 5,
    ATTACK_MOB = 1 << 6,
    BERSERK = 1 << 7,
    STONE_SKIN = 1 << 8,
    GOD_SPEED = 1 << 9,
    DEATH_BLOW = 1 << 10,
    REVIVE = 1 << 11,
}
