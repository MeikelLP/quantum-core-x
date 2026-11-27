namespace QuantumCore.API.Game.Types;

[Flags]
public enum EMapAttributes : uint
{
    NONE = 0,
    BLOCK = 1 << 0, // collision detection
    WATER = 1 << 1,
    NON_PVP = 1 << 2, // AKA "BAN_PK"
    OBJECT = 1 << 7, // building
}
