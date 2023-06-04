using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0xce, EDirection.Incoming, Sequence = false)]
    public class ChannelRequest
    {
    }
}