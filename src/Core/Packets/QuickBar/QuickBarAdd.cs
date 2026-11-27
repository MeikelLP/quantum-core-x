using QuantumCore.Networking;
using QuantumCore.Packets.General;

namespace QuantumCore.Packets.QuickBar;

[Packet(0x10, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class QuickBarAdd
{
    [Field(0)] public byte Position { get; set; }
    [Field(1)] public QuickSlot Slot { get; set; } = new QuickSlot();
}
