using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xfd, EDirection.Outgoing)]
[PacketGenerator]
public partial class GCPhase
{
    [Field(0)] public byte Phase { get; set; }
}
