using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[ServerToClientPacket(0x07)]
public partial class LoginFailed
{
    [FixedSizeString(9)] public string Status { get; set; } = "";
}