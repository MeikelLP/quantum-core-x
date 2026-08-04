using QuantumCore.Networking;

namespace QuantumCore.API.Packets.Shop;

[Packet(0x26, EDirection.OUTGOING)]
[SubPacket(0x05, 0)]
[PacketGenerator]
public partial class ShopNotEnoughMoney
{
    [Field(0)] public ushort Size { get; set; }
}
