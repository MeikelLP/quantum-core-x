using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial class GCHandshake
{
    public uint Handshake {get;set;}
    public uint Time {get;set;}
    public uint Delta {get;set;}
}
