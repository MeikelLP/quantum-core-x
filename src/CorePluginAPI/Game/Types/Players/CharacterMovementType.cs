namespace QuantumCore.API.Game.Types.Players;

public enum CharacterMovementType : byte
{
    Wait = 0,
    Move = 1,
    Attack = 2,
    Combo = 3,
    MobSkill = 4,

    SkillFlag = 1 << 7
}
