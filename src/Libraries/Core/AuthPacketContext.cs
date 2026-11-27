namespace QuantumCore;

public record struct AuthPacketContext<TPacket>(TPacket Packet, IAuthConnection Connection);