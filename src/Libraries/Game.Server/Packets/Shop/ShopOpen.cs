using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop;

[Packet(0x26, EDirection.OUTGOING)]
[SubPacket(0x00, 1)]
[PacketGenerator]
public partial class ShopOpen
{
    [Field(0)] public ushort Size => (ushort) Items.Length;
    [Field(1)] public uint Vid { get; set; }

    [Field(2)] public ShopItem[] Items { get; set; } = new ShopItem[40];
}
