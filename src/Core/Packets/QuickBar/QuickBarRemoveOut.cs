using QuantumCore.Networking;

namespace QuantumCore.API.Packets.QuickBar;

[Packet(0x1D, EDirection.OUTGOING)]
[PacketGenerator]
public partial class QuickBarRemoveOut
{
    [Field(0)] public byte Position { get; set; }
}
