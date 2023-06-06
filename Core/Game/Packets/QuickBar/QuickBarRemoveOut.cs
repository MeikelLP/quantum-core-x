using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1D, EDirection.Outgoing)]
public class QuickBarRemoveOut
{
    [Field(0)]
    public byte Position { get; set; }
}