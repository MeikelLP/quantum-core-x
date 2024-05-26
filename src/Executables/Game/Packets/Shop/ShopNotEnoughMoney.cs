using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[ServerToClientPacket(0x26, 0x05)]
public readonly ref partial struct ShopNotEnoughMoney
{
    public readonly ushort Size;

    public ShopNotEnoughMoney(ushort size)
    {
        Size = size;
    }
}