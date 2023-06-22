using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x09, EDirection.Outgoing)]
    public class CreateCharacterFailure
    {
        [Field(0)]
        public byte Error { get; set; }
    }
}