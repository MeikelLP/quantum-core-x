using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x02, EDirection.Incoming, Sequence = true)]
    public class Attack
    {
        [Field(0)]
        public byte AttackType { get; set; }
        [Field(1)]
        public uint Vid { get; set; }
        [Field(2, ArrayLength = 2)]
        public byte[] Unknown { get; set; } = {0,0};
    }
}