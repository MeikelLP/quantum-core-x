using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x06, EDirection.Incoming, Sequence = true)]
    public class SelectCharacter
    {
        [Field(0)]
        public byte Slot { get; set; }
    }
}