namespace QuantumCore.API.Game.Types.Combat;

[Flags]
public enum EDamageFlags : byte
{
    NORMAL = 1 << 0, 
    POISON = 1 << 1,
    DODGE = 1 << 2,
    BLOCKED = 1 << 3,
    PIERCING = 1 << 4,
    CRITICAL = 1 << 5
}
