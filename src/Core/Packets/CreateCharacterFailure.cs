using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x09, EDirection.OUTGOING)]
[PacketGenerator]
public partial class CreateCharacterFailure
{
    [Field(0)] public byte Error { get; set; }
}
