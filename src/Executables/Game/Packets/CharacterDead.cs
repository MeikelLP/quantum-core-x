using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x0e)]
public readonly ref partial struct CharacterDead
{
    public readonly uint Vid;

    public CharacterDead(uint vid)
    {
        Vid = vid;
    }
}