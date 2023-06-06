using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1E, EDirection.Outgoing)]
public class QuickBarSwapOut
{
    [Field(0)]
    public byte Position1 { get; set; }
    [Field(1)]
    public byte Position2 { get; set; }
}