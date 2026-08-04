using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x3f, EDirection.OUTGOING)]
[PacketGenerator]
public partial class SetTarget
{
    [Field(0)] public uint TargetVid { get; set; }
    [Field(1)] public byte Percentage { get; set; }
}
