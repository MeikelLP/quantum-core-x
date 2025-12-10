namespace QuantumCore.API.Game.Types.Entities;

[Flags]
public enum ESpawnStateFlags : byte
{
    Dead = 1 << 0,
    Spawning = 1 << 1,
    HostilePvP = 1 << 3,
    InGroup = 1 << 4
}
