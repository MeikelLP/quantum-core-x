namespace QuantumCore.API.Core.Models;

public class GCHandshakeData
{
    public ulong Handshake { get; set; }
    public long Time { get; set; }
    public long Delta { get; set; }
}