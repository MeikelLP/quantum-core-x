using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x2D, EDirection.Outgoing)]
[PacketGenerator]
public partial class QuestScript
{
    [Field(0)] public ushort PacketSize => (ushort)Source.Length;
    [Field(1)]
    public byte Skin { get; set; }

    [Field(2)] 
    public ushort SourceSize { get; set; }
    public string Source { get; set; }
}