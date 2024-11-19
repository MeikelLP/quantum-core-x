using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x1B, EDirection.Outgoing)]
[PacketGenerator]
public partial class GroundItemRemove
{
    [Field(0)] public uint Vid { get; set; }
}