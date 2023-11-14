using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x10, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class QuickBarAdd
{
    [Field(0)]
    public byte Position { get; set; }
    [Field(1)]
    public QuickSlot Slot { get; set; } = new QuickSlot();
}
