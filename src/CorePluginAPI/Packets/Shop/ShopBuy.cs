using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x32, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x01, 0)]
[PacketGenerator]
public partial class ShopBuy
{
    public byte Count { get; set; }
    public byte Position { get; set; }
}
