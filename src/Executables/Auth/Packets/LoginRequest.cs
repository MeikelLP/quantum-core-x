using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[ClientToServerPacket(0x6f, HasSequence = true)]
public partial class LoginRequest
{
    [FixedSizeString(31)] public string Username { get; set; }

    [FixedSizeString(17)] public string Password { get; set; }

    [FixedSizeArray(4)] public uint[] EncryptKey { get; set; }
}