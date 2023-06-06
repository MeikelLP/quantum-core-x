using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x3d, EDirection.Incoming, Sequence = true)]
    public class TargetChange
    {
        [Field(0)]
        public uint TargetVid { get; set; }
    }
}