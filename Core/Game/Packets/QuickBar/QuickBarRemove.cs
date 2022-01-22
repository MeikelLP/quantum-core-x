using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x11, EDirection.Incoming, Sequence = true)]
public class QuickBarRemove
{
    [Field(0)]
    public byte Position { get; set; }
}