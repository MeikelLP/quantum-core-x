using QuantumCore.Core.Packets;
using QuantumCore.Game.Packets.General;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x10, EDirection.Incoming, Sequence = true)]
public class QuickBarAdd
{
    [Field(0)]
    public byte Position { get; set; }
    [Field(1)]
    public QuickSlot Slot { get; set; }
}