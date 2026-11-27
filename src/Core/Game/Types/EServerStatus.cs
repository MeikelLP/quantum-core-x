namespace QuantumCore.API.Game.Types;

public enum EServerStatus : byte
{
    OFFLINE = 0,
    ONLINE = 1,
    ONLINE_BUSY = 2,
    ONLINE_FULL = 3
}
