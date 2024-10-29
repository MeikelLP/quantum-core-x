using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop
{
    [Packet(0x26, EDirection.Outgoing)]
    [SubPacket(0x00, 0)]
    [PacketGenerator]
    public partial class ShopOpen
    {
        public ushort Size => (ushort) Items.Length;
        public uint Vid { get; set; }

        public ShopItem[] Items { get; set; } = new ShopItem[40];
    }
}