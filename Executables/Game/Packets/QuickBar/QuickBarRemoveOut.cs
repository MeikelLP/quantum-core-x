using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[Packet(0x1D, EDirection.Outgoing)]
[PacketGenerator]
public partial class QuickBarRemoveOut
{
    [Field(0)]
    public byte Position { get; set; }
}