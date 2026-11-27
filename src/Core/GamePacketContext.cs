namespace QuantumCore.API;

public record struct GamePacketContext<TPacket>(TPacket Packet, IGameConnection Connection);