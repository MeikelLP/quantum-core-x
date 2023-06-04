namespace QuantumCore.API;

public struct GamePacketContext<TPacket>
{
    public TPacket Packet { get; set; }

    public IGameConnection Connection { get; set; }
}

public struct AuthPacketContext<TPacket>
{
    public TPacket Packet { get; set; }

    public IAuthConnection Connection { get; set; }
}