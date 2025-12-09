namespace QuantumCore.API.Game.Types.Players;

public enum CharacterMovementType
{
    Wait = 0,
    Move = 1,
    Attack = 2,
    Combo = 3,
    MobSkill = 4,
    Max = 6,
    Skill = 0x80
}
