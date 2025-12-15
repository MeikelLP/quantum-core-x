using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0B, EDirection.OUTGOING)]
[PacketGenerator]
public partial class DeleteCharacterFail
{
}
