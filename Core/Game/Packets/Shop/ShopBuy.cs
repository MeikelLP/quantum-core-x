using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Shop
{
    [Packet(0x32, EDirection.Incoming, Sequence = true)]
    [SubPacket(0x01, 0)]
    public class ShopBuy
    {
        [Field(1)]
        public byte Count { get; set; }
        [Field(2)]
        public byte Position { get; set; }
    }
}