using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x2D, EDirection.Outgoing)]
public class QuestScript
{
    [Size]
    [Field(0)]
    public ushort Size { get; set; }
    [Field(1)]
    public byte Skin { get; set; }
    [Field(2)]
    public ushort SourceSize { get; set; }
    [Dynamic]
    public string Source { get; set; }
}