using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x1a, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class ClickNpc
    {
        [Field(0)]
        public uint Vid { get; set; }
    }
}