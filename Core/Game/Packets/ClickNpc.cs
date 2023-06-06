using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x1a, EDirection.Incoming, Sequence = true)]
    public class ClickNpc
    {
        [Field(0)]
        public uint Vid { get; set; }
    }
}