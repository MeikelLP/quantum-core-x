namespace QuantumCore.API.Game.Types.Entities;

[Flags]
public enum ESpawnStateFlags : byte
{
    DEAD = 1 << 0,
    SPAWNING = 1 << 1,
    HOSTILE_PV_P = 1 << 3,
    IN_GROUP = 1 << 4
}
