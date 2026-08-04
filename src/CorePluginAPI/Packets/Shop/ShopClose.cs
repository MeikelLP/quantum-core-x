using QuantumCore.Networking;

namespace QuantumCore.API.Packets.Shop;

[Packet(0x32, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x00, 0)]
[PacketGenerator]
public partial class ShopClose
{
}
