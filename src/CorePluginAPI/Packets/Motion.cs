using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x24, EDirection.OUTGOING)]
[PacketGenerator]
public partial class Motion
{
    [Field(0)] public uint Vid { get; set; }
    [Field(1)] public uint VictimVid { get; set; }
    [Field(2)] public ushort MotionId { get; set; }
}