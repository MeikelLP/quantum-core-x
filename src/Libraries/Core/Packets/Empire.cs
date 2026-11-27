using QuantumCore.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Packets;

[Packet(0x5a, EDirection.INCOMING | EDirection.OUTGOING, Sequence = true)]
[PacketGenerator]
public partial class Empire
{
    [Field(0)] public EEmpire EmpireId { get; set; }
}
