using QuantumCore.Networking;

namespace QuantumCore.Packets;

[Packet(0x6a, EDirection.OUTGOING)]
[PacketGenerator]
public partial class GameTime
{
    [Field(0)] public uint Time { get; set; }
}
