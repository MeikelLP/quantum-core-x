using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets
{
    [Packet(0xfd, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class GCPhase
    {
        [Field(0)] public EPhase Phase { get; set; }
    }
}