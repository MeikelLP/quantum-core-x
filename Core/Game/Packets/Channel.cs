using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x79, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class Channel
    {
        [Field(0)]
        public byte ChannelNo { get; set; }
    }
}