using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[ServerToClientPacket(0x26, 0x07)]
public readonly ref partial struct ShopNoSpaceLeft
{
    public readonly ushort Size;

    public ShopNoSpaceLeft(ushort size)
    {
        Size = size;
    }
}