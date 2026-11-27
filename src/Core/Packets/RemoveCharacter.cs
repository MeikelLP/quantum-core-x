using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x02, EDirection.OUTGOING)]
[PacketGenerator]
public partial class RemoveCharacter
{
    [Field(0)] public uint Vid { get; set; }
}
