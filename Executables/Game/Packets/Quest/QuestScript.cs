using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x2D, EDirection.Outgoing)]
[PacketGenerator]
public partial class QuestScript
{
    [Size]
    [Field(0)]
    public ushort Size { get; set; }
    [Field(1)]
    public byte Skin { get; set; }

    [Field(2)] public ushort SourceSize => (ushort)Source.Length;
    public string Source { get; set; }
    
    public byte EndOfSource => 0x00;
}