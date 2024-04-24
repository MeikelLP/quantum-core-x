using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop
{
    [Packet(0x32, EDirection.Outgoing)]
    [SubPacket(0x00, 1)]
    [PacketGenerator]
    public partial class ShopOpen
    {

        public ushort Size => (ushort)Items.Length;
        public uint Vid { get; set; }

        public ShopItem[] Items { get; set; } = new ShopItem[40]
        {
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
            new ShopItem(),
        };
    }
}