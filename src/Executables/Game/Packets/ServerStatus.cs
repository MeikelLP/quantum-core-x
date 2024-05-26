namespace QuantumCore.Game.Packets;

public readonly struct ServerStatus
{
    public readonly short Port;
    public readonly byte Status;

    public ServerStatus(short port, byte status)
    {
        Port = port;
        Status = status;
    }
}