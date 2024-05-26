namespace QuantumCore.Networking;

public interface IPacket
{
    void Deserialize(ReadOnlySpan<byte> buffer);
}