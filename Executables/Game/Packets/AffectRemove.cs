using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Affects
{
    [Packet(0x7F, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class AffectRemove
    {
        [Field(0)]
        public uint Type { get; set; }
        [Field(1)]
        public byte ApplyOn { get; set; }
    }
}
