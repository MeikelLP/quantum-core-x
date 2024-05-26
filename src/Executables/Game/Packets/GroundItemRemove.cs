using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x1B)]
public readonly ref partial struct GroundItemRemove
{
    public readonly uint Vid;

    public GroundItemRemove(uint vid)
    {
        Vid = vid;
    }
}