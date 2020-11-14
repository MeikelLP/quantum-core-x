using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x08, EDirection.Outgoing)]
    public class CreateCharacterSuccess
    {
        [Field(0)]
        public byte Slot { get; set; }
        [Field(1)]
        public Character Character { get; set; }
    }
}