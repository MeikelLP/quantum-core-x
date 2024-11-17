using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0xFE, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class Pong
    {
    }
}