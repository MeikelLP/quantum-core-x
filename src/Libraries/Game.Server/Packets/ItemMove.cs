using QuantumCore.API.Game.Types.Items;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0d, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class ItemMove
{
    [Field(0)] public WindowType FromWindow { get; set; }
    [Field(1)] public ushort FromPosition { get; set; }
    [Field(2)] public WindowType ToWindow { get; set; }
    [Field(3)] public ushort ToPosition { get; set; }
    [Field(4)] public byte Count { get; set; }
}
