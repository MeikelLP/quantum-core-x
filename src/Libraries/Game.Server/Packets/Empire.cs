using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x5a, EDirection.Incoming | EDirection.Outgoing, Sequence = true)]
[PacketGenerator]
public partial class Empire
{
    [Field(0)] public EEmpire EmpireId { get; set; }
}