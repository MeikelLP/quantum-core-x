using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[ServerToClientPacket(0x96)]
public partial class LoginSuccess
{
    public uint Key { get; set; }
    public byte Result { get; set; }
}