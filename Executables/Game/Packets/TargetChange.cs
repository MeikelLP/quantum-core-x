using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x3d, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class TargetChange
    {
        [Field(0)]
        public uint TargetVid { get; set; }
    }
}