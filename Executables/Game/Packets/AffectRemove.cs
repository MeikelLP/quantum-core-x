using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Affects
{
    [Packet(0x7F, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class AffectRemove
    {
        public uint Type { get; set; }
        public byte ApplyOn { get; set; }
    }
}
