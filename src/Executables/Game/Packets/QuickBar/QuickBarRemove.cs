using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ClientToServerPacket(0x11, HasSequence = true)]
public readonly ref partial struct QuickBarRemove
{
    public readonly byte Position;

    public QuickBarRemove(byte position)
    {
        Position = position;
    }
}