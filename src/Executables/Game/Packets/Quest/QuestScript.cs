using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

[ServerToClientPacket(0x2D)]
public readonly ref partial struct QuestScript
{
    public readonly ushort PacketSize;
    public readonly byte Skin;
    public readonly ushort SourceSize;
    public readonly string Source;

    public QuestScript(byte skin, ushort sourceSize, string source)
    {
        Skin = skin;
        SourceSize = sourceSize;
        Source = source;
        PacketSize = (ushort) Source.Length;
    }
}