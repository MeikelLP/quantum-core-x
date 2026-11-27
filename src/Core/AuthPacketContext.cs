namespace QuantumCore.API;

public record struct AuthPacketContext<TPacket>(TPacket Packet, IAuthConnection Connection);