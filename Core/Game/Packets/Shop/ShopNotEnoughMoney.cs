using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x26, EDirection.Outgoing)]
[SubPacket(0x05, 1)]
public class ShopNotEnoughMoney
{
    [Field(0)]
    [Size]
    public ushort Size { get; set; }
}