using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets
{
    [Packet(0x6f, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class LoginRequest
    {
        [Field(0, Length = 31)] public string Username { get; set; } = "";

        [Field(1, Length = 17)] public string Password { get; set; } = "";

        [Field(2, ArrayLength = 4)] public uint[] EncryptKey { get; set; } = new uint[4];
    }
}