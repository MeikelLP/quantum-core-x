namespace QuantumCore.API.Game.Types
{
    public enum EPhases : byte
    {
        Handshake = 1,
        Login = 2,
        Select = 3,
        Loading = 4,
        Game = 5,
        Auth = 10
    }
}