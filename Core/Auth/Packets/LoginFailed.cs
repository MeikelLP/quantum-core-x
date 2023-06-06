using QuantumCore.Core.Networking;

namespace QuantumCore.Auth.Packets
{
    [Packet(0x07, EDirection.Outgoing)]
    public class LoginFailed
    {
        [Field(1, Length = 9)]
        public string Status { get; set; }
    }
}