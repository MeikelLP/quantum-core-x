using QuantumCore.Game;

namespace QuantumCore.Core.Networking;

public struct PacketContext<T>
{
    public T Packet { get; set; }

    public GameConnection Connection { get; set; }
}