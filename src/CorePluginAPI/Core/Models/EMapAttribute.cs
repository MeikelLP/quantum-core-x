namespace QuantumCore.API.Core.Models;

[Flags]
public enum EMapAttribute : uint
{
    None = 0,
    Block = 1 << 0, // collision detection
    Water = 1 << 1,
    NonPvp = 1 << 2, // AKA "BAN_PK"
    Object = 1 << 7, // building
}
