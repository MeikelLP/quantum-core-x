using QuantumCore.Networking;

namespace QuantumCore.API.Packets.QuickBar;

[Packet(0x11, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class QuickBarRemove
{
    [Field(0)] public byte Position { get; set; }
}
