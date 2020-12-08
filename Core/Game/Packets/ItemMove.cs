using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0d, EDirection.Incoming, Sequence = true)]
    public class ItemMove
    {
        [Field(0)]
        public byte FromWindow { get; set; }
        [Field(1)]
        public ushort FromPosition { get; set; }
        [Field(2)]
        public byte ToWindow { get; set; }
        [Field(3)]
        public ushort ToPosition { get; set; }
        [Field(4)]
        public byte Count { get; set; }
    }
}