using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[ClientToServerPacket(0x32, 0x01, HasSequence = true)]
public readonly ref partial struct ShopBuy
{
    public readonly byte Count;
    public readonly byte Position;

    public ShopBuy(byte count, byte position)
    {
        Count = count;
        Position = position;
    }
}