using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ServerToClientPacket(0x1E)]
public readonly ref partial struct QuickBarSwapOut
{
    public readonly byte Position1;
    public readonly byte Position2;

    public QuickBarSwapOut(byte position1, byte position2)
    {
        Position1 = position1;
        Position2 = position2;
    }
}