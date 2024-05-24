using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop
{
    [ServerToClientPacket(0x26, 0x00)]
    public readonly ref partial struct ShopOpen
    {
        public readonly ushort Size;
        public readonly uint Vid;
        [FixedSizeArray(40)] public readonly ShopItem[] Items;

        public ShopOpen(uint vid, ShopItem[] items)
        {
            Size = (ushort) items.Length;
            Vid = vid;
            Items = items;
        }
    }
}