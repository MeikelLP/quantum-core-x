namespace QuantumCore.API.Game.Types;

public enum EServerStatus : byte
{
    Offline = 0,
    Online = 1,
    OnlineBusy = 2,
    OnlineFull = 3
}
