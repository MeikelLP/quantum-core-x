using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0a, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class EnterGame
    {
    }
}