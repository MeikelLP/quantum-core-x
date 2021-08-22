using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0A, EDirection.Outgoing)]
    public class DeleteCharacterSuccess
    {
        [Field(0)]
        public byte Slot { get; set; }
    }
}
