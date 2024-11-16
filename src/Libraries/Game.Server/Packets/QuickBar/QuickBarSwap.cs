using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x12, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class QuickBarSwap
{
    [Field(0)] public byte Position1 { get; set; }
    [Field(1)] public byte Position2 { get; set; }
}