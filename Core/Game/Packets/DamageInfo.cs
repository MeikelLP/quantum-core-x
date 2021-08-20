using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x87, EDirection.Outgoing)]
    public class DamageInfo
    {
        [Field(0)]
        public uint Vid { get; set; }
        [Field(1)]
        public byte DamageType { get; set; }
        [Field(2)]
        public int Damage { get; set; }
    }
}