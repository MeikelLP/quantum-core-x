using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets;

[Packet(0x0F, EDirection.Incoming, Sequence = true)]
public class ItemPickup
{
    [Field(0)]
    public uint Vid { get; set; }
}