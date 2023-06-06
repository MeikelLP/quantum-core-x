using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0b, EDirection.Incoming, Sequence = true)]
    public class ItemUse
    {
        [Field(0)]
        public byte Window { get; set; }
        [Field(1)]
        public ushort Position { get; set; }
    }
}
