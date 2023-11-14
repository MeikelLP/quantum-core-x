using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1C, EDirection.Outgoing)]
[PacketGenerator]
public partial class QuickBarAddOut
{
    [Field(0)]
    public byte Position { get; set; }
    [Field(1)]
    public QuickSlot Slot { get; set; } = new QuickSlot();
}
