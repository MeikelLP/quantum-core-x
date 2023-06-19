using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record GCHandshake(uint Handshake, uint Time, uint Delta);