namespace QuantumCore.Game.Packets;

public enum CharacterMovementType : byte
{
    Wait = 0,
    Move = 1,
    Attack = 2,
    Combo = 3,
    MobSkill = 4,
    Max,
    Skill = 0x80
}
