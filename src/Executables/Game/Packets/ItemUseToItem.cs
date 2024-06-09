using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(60, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class ItemUseToItem
    {
        [Field(0)]
        public byte Window { get; set; }
        [Field(1)]
        public ushort Position { get; set; }
        [Field(2)]
        public ushort TargetPosition { get; set; }
    }
}
