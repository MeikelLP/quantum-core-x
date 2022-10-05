namespace QuantumCore.Core.Packets
{
    [Packet(0xfd, EDirection.Outgoing)]
    public class GCPhase
    {
        [Field(0)] public byte Phase { get; set; }
    }
}