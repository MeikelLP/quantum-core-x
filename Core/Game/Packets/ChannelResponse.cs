using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0xd2, EDirection.Outgoing)]
    public class ChannelResponse
    {
        [Field(0, Length = 5)]
        public ushort Port { get; set; }
        [Field(1)]
        public byte Status { get; set; }
    }
}