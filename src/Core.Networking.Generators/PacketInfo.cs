namespace QuantumCore.Networking;

public record struct PacketInfo(
    byte Header,
    byte? SubHeader,
    bool HasSequence);