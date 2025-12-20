using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x05, EDirection.OUTGOING)]
[PacketGenerator]
public partial class SyncPositionsOut
{
    // Reference Positions so generator links this as the size field, but use GetSize() for actual value.
    [Field(0)] public ushort TotalSize => (ushort)(GetSize() + Positions.Length * 0);
    
    [Field(1)] public SyncPositionElement[] Positions { get; init; } = [];
}
