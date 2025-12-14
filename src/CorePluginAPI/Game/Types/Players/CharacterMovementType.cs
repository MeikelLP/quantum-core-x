namespace QuantumCore.API.Game.Types.Players;

public enum CharacterMovementType : byte
{
    WAIT = 0,
    MOVE = 1,
    ATTACK = 2,
    COMBO = 3,
    MOB_SKILL = 4,

    SKILL_FLAG = 1 << 7
}
