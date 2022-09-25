namespace QuantumCore.API;

public struct PacketContext<T>
{
    public T Packet { get; set; }

    public IGameConnection Connection { get; set; }
}