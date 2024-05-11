using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Mall;

[Packet(0x7A, EDirection.Outgoing)]
[PacketGenerator]
public partial class MallboxSize
{
    [Field(0)]
    public byte Size { get; set; }
}
