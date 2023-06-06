using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x1A, EDirection.Outgoing)]
public class GroundItemAdd
{
    [Field(0)]
    public int PositionX { get; set; }
    
    [Field(1)]
    public int PositionY { get; set; }
    
    [Field(2)]
    public int PositionZ { get; set; }
    
    [Field(3)]
    public uint Vid { get; set; }
    
    [Field(4)]
    public uint ItemId { get; set; }
}