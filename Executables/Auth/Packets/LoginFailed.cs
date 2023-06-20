using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets
{
    [Packet(0x07, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class LoginFailed
    {
        [Field(0)]
        public byte Unknown { get; set; }

        [Field(1, Length = 9)]
        public string Status { get; set; }
    }
}