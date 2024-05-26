using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[ClientToServerPacket(0x32, 0x03, HasSequence = true)]
public readonly ref partial struct ShopSell
{
    public readonly byte Position;
    public readonly byte Count;

    public ShopSell(byte position, byte count)
    {
        Position = position;
        Count = count;
    }
}