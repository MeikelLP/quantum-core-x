using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x26, EDirection.OUTGOING)]
[SubPacket(0x07, 0)]
[PacketGenerator]
public partial class ShopNoSpaceLeft
{
}
