using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record GcHandshake(uint Handshake, uint Time, uint Delta);