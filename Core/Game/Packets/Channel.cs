using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x79, EDirection.Outgoing)]
    public class Channel
    {
        [Field(0)]
        public byte ChannelNo { get; set; }
    }
}