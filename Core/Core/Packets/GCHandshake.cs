namespace QuantumCore.Core.Packets
{
    [Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
    public class GCHandshake
    {
        [Field(0)] public uint Handshake { get; set; }

        [Field(1)] public uint Time { get; set; }

        [Field(2)] public uint Delta { get; set; }
    }
}