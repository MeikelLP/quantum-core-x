using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1E, EDirection.Outgoing)]
[PacketGenerator]
public partial class QuickBarSwapOut
{
    [Field(0)] public byte Position1 { get; set; }
    [Field(1)] public byte Position2 { get; set; }
}