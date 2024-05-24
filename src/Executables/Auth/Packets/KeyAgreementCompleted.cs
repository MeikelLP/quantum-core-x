using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[ClientToServerPacket(0xFA, HasSequence = true)]
[ServerToClientPacket(0xFA, HasSequence = true)]
public partial class KeyAgreementCompleted
{
}