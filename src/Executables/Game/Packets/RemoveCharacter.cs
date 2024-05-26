using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x02)]
public readonly ref partial struct RemoveCharacter
{
    public readonly uint Vid;

    public RemoveCharacter(uint vid)
    {
        Vid = vid;
    }
}