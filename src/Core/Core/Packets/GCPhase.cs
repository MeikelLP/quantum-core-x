using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[ServerToClientPacket(0xfd)]
public partial class GCPhase
{
    public EPhases Phase { get; set; }
}