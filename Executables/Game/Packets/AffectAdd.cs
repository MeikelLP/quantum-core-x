using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Affects
{
    [Packet(0x7E, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class AffectAdd
    {
        [Field(0, ArrayLength = 1)]
        public AffectAddPacket[] Elem { get; set; } = new AffectAddPacket[1];
    }

    public class AffectAddPacket
    {
        public uint Type { get; set; }
        public byte ApplyOn { get; set; }
        public uint ApplyValue { get; set; }
        public uint Flag { get; set; }
        public uint Duration { get; set; }
        public uint SpCost { get; set; }
    }
}
