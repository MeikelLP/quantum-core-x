using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x32, EDirection.Incoming, Sequence = true)]
[SubPacket(0x03, 0)]
public class ShopSell
{
    [Field(1)]
    public byte Position { get; set; }
    [Field(2)]
    public byte Count { get; set; }
}