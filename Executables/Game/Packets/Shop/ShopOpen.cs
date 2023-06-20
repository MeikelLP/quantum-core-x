using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Shop
{
    [Packet(0x26, EDirection.Outgoing)]
    [SubPacket(0x00, 1)]
    public class ShopOpen
    {
        
        [Field(0)]
        [Size]
        public ushort Size { get; set; }
        
        [Field(2)]
        public uint Vid { get; set; }

        [Field(3, ArrayLength = 40)]
        public ShopItem[] Items { get; set; } = new ShopItem[40];
    }
}