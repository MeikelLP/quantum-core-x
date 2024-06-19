using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Mall;

[Packet(0x45, EDirection.Incoming)]
[PacketGenerator]
public partial class MallMoveItem
{
    [Field(0)]
    public uint WindowPosition { get; set; }
    
    [Field(1)]
    public MallItemPosition ItemPosition { get; set; }
}
