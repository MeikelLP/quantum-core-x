using QuantumCore.Networking;

namespace QuantumCore.Packets.Quest;

[Packet(0x1D, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class QuestAnswer
{
    [Field(0)] public byte Answer { get; set; }
}
