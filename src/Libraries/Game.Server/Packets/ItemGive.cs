using QuantumCore.API.Game.Types.Items;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x53, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class ItemGive
{
    [Field(0)] public uint TargetVid { get; set; }
    [Field(1)] public WindowType Window { get; set; }
    [Field(2)] public ushort Position { get; set; }
    [Field(3)] public byte Count { get; set; }
}
