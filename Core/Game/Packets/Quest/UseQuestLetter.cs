using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x42, EDirection.Incoming, Sequence = true)]
public class UseQuestLetter
{
    /// <summary>
    /// This might actually be used for more than the quest letter, because the upper bit is always 1...
    /// </summary>
    [Field(0)]
    public uint LetterId { get; set; }
}