using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x14, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class ItemDrop
{
    [Field(0)]
    public byte Window { get; set; }
    
    [Field(1)]
    public ushort Position { get; set; }
    
    [Field(2)]
    public uint Gold { get; set; }
    
    [Field(3)]
    public byte Count { get; set; }
}