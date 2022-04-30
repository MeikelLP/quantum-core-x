using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets;

[Packet(0x1B, EDirection.Outgoing)]
public class GroundItemRemove
{
    [Field(0)]
    public uint Vid { get; set; }
}