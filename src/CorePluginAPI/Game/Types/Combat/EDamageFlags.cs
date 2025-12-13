namespace QuantumCore.API.Game.Types.Combat;

[Flags]
public enum EDamageFlags : byte
{
    Normal = 1 << 0, 
    Poison = 1 << 1,
    Dodge = 1 << 2,
    Blocked = 1 << 3,
    Piercing = 1 << 4,
    Critical = 1 << 5
}
