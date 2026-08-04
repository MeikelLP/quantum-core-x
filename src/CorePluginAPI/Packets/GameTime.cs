using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x6a, EDirection.OUTGOING)]
[PacketGenerator]
public partial class GameTime
{
    [Field(0)] public uint Time { get; set; }
}
