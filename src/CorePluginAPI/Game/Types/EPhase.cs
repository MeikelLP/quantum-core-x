namespace QuantumCore.API.Game.Types;

public enum EPhase : byte
{
    HANDSHAKE = 1,
    LOGIN = 2,
    SELECT = 3,
    LOADING = 4,
    GAME = 5,
    AUTH = 10
}
