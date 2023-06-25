namespace QuantumCore.API;

public struct GamePacketContext<TPacket>
{
    public TPacket Packet { get; set; }

    public IGameConnection Connection { get; set; }
}