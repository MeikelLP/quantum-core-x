using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0xfd)]
public readonly ref partial struct GCPhase
{
    public readonly EPhases Phase;

    public GCPhase(EPhases phase)
    {
        Phase = phase;
    }
}