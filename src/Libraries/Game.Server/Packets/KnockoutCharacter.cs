using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0d, EDirection.OUTGOING)]
[PacketGenerator]
public partial class KnockoutCharacter
{
    [Field(0)] public uint Vid { get; set; }
}
