using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x06, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class SelectCharacter
    {
        [Field(0)]
        public byte Slot { get; set; }
    }
}