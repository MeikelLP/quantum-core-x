using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x41, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class Warp
    {
        [Field(0)]
        public int PositionX { get; set; }
        
        [Field(1)]
        public int PositionY { get; set; }
        
        [Field(2)]
        public int ServerAddress { get; set; }
        
        [Field(3)]
        public ushort ServerPort { get; set; }
    }
}