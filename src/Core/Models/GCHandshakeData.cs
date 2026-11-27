namespace QuantumCore.API.Core.Models;

public class GcHandshakeData
{
    public ulong Handshake { get; set; }
    public TimeSpan Time { get; set; }
    public TimeSpan Delta { get; set; }
}
