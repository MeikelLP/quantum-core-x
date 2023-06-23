using QuantumCore.Core.Packets;
using QuantumCore.Game.Packets.General;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1C, EDirection.Outgoing)]
public class QuickBarAddOut
{
    [Field(0)]
    public byte Position { get; set; }
    [Field(1)]
    public QuickSlot Slot { get; set; }
}