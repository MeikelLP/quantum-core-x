using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

[ClientToServerPacket(0x1D, HasSequence = true)]
public readonly ref partial struct QuestAnswer
{
    public readonly byte Answer;

    public QuestAnswer(byte answer)
    {
        Answer = answer;
    }
}