using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0A, EDirection.OUTGOING)]
[PacketGenerator]
public partial class DeleteCharacterSuccess
{
    [Field(0)] public byte Slot { get; set; }
}
