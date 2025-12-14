using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xfd, EDirection.Outgoing)]
[PacketGenerator]
public partial class GcPhase
{
    [Field(0)] public EPhase Phase { get; set; }
}