using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x1a, HasSequence = true)]
public readonly ref partial struct ClickNpc
{
    public readonly uint Vid;

    public ClickNpc(uint vid)
    {
        Vid = vid;
    }
}