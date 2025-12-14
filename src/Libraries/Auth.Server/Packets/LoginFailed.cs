using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[Packet(0x07, EDirection.OUTGOING)]
[PacketGenerator]
public partial class LoginFailed
{
    [Field(0, Length = 9)] public string Status { get; set; } = "";
}
