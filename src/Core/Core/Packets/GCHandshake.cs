using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[ClientToServerPacket(0xff)]
[ServerToClientPacket(0xff)]
public partial class GCHandshake
{
    public uint Handshake { get; set; }
    public uint Time { get; set; }
    public uint Delta { get; set; }
}