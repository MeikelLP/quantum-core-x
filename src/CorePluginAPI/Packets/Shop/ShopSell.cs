using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x32, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x03, 0)]
[PacketGenerator]
public partial class ShopSell
{
    public byte Position { get; set; }
    public byte Count { get; set; }
}
