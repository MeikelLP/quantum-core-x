using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Mall;

[Packet(0x80, EDirection.Outgoing, Sequence = true)]
[PacketGenerator]
public partial class MallItem
{
    [Field(0)]
    public MallItemPosition Cell { get; set; } = new MallItemPosition();
    
    [Field(1)]
    public uint Vid { get; set; }
    
    [Field(2)]
    public byte Count { get; set; }
    
    [Field(3)]
    public uint Flags { get; set; }
    
    [Field(4)]
    public uint AntiFlags { get; set; }

    [Field(5)]
    public byte Highlight { get; set; }
    
    [Field(6, ArrayLength = 3)] 
    public uint[] Sockets { get; set; } = new uint[3];
    
    [Field(7, ArrayLength = 7)]
    public ItemBonus[] Bonuses { get; set; } = new ItemBonus[7];
}

public class MallItemPosition
{
    [Field(0)]
    public byte Type { get; set; }
    
    [Field(1)]
    public ushort Cell { get; set; }
}
