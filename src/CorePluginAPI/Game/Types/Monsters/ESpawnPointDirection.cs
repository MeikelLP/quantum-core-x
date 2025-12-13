namespace QuantumCore.API.Game.Types.Monsters;

/// <summary>
/// Spawn point directions follows the compass rose with counter-clockwise increments.
/// </summary>
public enum ESpawnPointDirection : byte
{
    Random = 0,
    South = 1,
    SouthEast = 2,
    East = 3,
    NorthEast = 4,
    North = 5,
    NorthWest = 6,
    West = 7,
    SouthWest = 8
}
