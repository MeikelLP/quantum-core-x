using QuantumCore.Core.Packets;

namespace QuantumCore.Auth.Packets
{
    [Packet(0x6f, EDirection.Incoming)]
    internal class LoginRequest
    {
        [Field(0, 31)] public string Username { get; set; }

        [Field(1, 17)] public string Password { get; set; }

        [Field(2, 4)] public uint[] EncryptKey { get; set; }
    }
}