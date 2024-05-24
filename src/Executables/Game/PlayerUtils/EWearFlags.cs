namespace QuantumCore.Game.PlayerUtils;

[Flags]
public enum EWearFlags
{
    Body = (1 << 0),
    Head = (1 << 1),
    Shoes = (1 << 2),
    Bracelet = (1 << 3),
    Weapon = (1 << 4),
    Necklace = (1 << 5),
    Earrings = (1 << 6),
    Unique = (1 << 7),
    Shield = (1 << 8),
    Arrow = (1 << 9)
}