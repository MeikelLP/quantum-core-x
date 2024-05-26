using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ServerToClientPacket(0x1D)]
public readonly ref partial struct QuickBarRemoveOut
{
    public readonly byte Position;

    public QuickBarRemoveOut(byte position)
    {
        Position = position;
    }
}