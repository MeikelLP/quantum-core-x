using QuantumCore.API.Game.Types.Items;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0b, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class ItemUse
{
    [Field(0)] public WindowType Window { get; set; }
    [Field(1)] public ushort Position { get; set; }
}
