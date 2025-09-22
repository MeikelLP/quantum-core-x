namespace QuantumCore.API.Core.Models;

[Flags]
public enum EMapAttribute : uint
{
    None = 0,
    Block = 1 << 0, // collision detection
    Water = 1 << 1,
    BanPk = 1 << 2, // ban player kill AKA "No PvP"
    Object = 1 << 7, // building
}
