using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.INCOMING | EDirection.OUTGOING)]
[PacketGenerator]
public partial record GcHandshake(uint Handshake, uint Time, uint Delta);
