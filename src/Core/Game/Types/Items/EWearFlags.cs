namespace QuantumCore.API.Game.Types.Items;

[Flags]
public enum EWearFlags
{
    BODY = (1 << 0),
    HEAD = (1 << 1),
    SHOES = (1 << 2),
    BRACELET = (1 << 3),
    WEAPON = (1 << 4),
    NECKLACE = (1 << 5),
    EARRINGS = (1 << 6),
    UNIQUE = (1 << 7),
    SHIELD = (1 << 8),
    ARROW = (1 << 9),
}
