using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[ClientToServerPacket(0x32, 0x00, HasSequence = true)]
public readonly ref partial struct ShopClose
{
}