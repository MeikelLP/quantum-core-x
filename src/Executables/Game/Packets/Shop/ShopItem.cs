using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

public readonly struct ShopItem
{
    public readonly uint ItemId;
    public readonly uint Price;
    public readonly byte Count;
    public readonly byte Position;
    [FixedSizeArray(3)] public readonly uint[] Sockets;
    [FixedSizeArray(7)] public readonly ItemBonus[] Bonuses;

    public ShopItem(uint itemId, uint price, byte count, byte position, uint[] sockets, ItemBonus[] bonuses)
    {
        ItemId = itemId;
        Price = price;
        Count = count;
        Position = position;
        Sockets = sockets;
        Bonuses = bonuses;
    }
}