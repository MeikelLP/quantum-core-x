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
        [Field(0)]
        public uint Type { get; set; }
        [Field(1)]
        public byte ApplyOn { get; set; }
        [Field(2)]
        public uint ApplyValue { get; set; }
        [Field(3)]
        public uint Flag { get; set; }
        [Field(4)]
        public uint Duration { get; set; }
        [Field(5)]
        public uint SpCost { get; set; } = 0;
    }
}
