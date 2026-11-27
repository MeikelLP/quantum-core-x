namespace QuantumCore.API.Game.Types.Monsters;

/// <summary>
/// Spawn point directions follows the compass rose with counter-clockwise increments.
/// </summary>
public enum ESpawnPointDirection : byte
{
    RANDOM = 0,
    SOUTH = 1,
    SOUTH_EAST = 2,
    EAST = 3,
    NORTH_EAST = 4,
    NORTH = 5,
    NORTH_WEST = 6,
    WEST = 7,
    SOUTH_WEST = 8
}
