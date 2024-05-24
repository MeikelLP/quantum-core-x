using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x10)]
public readonly ref partial struct CharacterPoints
{
    [FixedSizeArray(255)] public readonly uint[] Points;

    public CharacterPoints(uint[] points)
    {
        Points = points;
    }
}