namespace QuantumCore;

public record struct GamePacketContext<TPacket>(TPacket Packet, IGameConnection Connection);