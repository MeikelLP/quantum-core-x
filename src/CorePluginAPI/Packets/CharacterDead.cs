using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x0e, EDirection.OUTGOING)]
[PacketGenerator]
public partial class CharacterDead
{
    [Field(0)] public uint Vid { get; set; }
}
