using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0xf1, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class Version
    {
        [Field(0, Length = 33)] public string ExecutableName { get; set; } = "";
        [Field(1, Length = 33)] public string Timestamp { get; set; } = "";
    }
}