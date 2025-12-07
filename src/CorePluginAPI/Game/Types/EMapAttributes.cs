namespace QuantumCore.API.Game.Types;

[Flags]
public enum EMapAttributes : uint
{
    None = 0,
    Block = 1 << 0, // collision detection
    Water = 1 << 1,
    NonPvp = 1 << 2, // AKA "BAN_PK"
    Object = 1 << 7, // building
}
