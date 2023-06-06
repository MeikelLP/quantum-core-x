using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x1D, EDirection.Incoming, Sequence = true)]
public class QuestAnswer
{
    [Field(0)]
    public byte Answer { get; set; }
}