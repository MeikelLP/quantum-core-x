using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ClientToServerPacket(0x12, HasSequence = true)]
public readonly ref partial struct QuickBarSwap
{
    public readonly byte Position1;
    public readonly byte Position2;

    public QuickBarSwap(byte position1, byte position2)
    {
        Position1 = position1;
        Position2 = position2;
    }
}