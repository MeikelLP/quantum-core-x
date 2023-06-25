namespace QuantumCore.API;

public struct AuthPacketContext<TPacket>
{
    public TPacket Packet { get; set; }

    public IAuthConnection Connection { get; set; }
}