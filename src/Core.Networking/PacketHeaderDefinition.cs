namespace QuantumCore.Networking;

public record struct PacketHeaderDefinition(byte Header, byte? SubHeader = null)
{
    public static implicit operator PacketHeaderDefinition(byte header)
    {
        return new PacketHeaderDefinition(header);
    }

    public static implicit operator PacketHeaderDefinition(ValueTuple<byte, byte?> tuple)
    {
        return new PacketHeaderDefinition(tuple.Item1, tuple.Item2);
    }

    public override string ToString()
    {
        var subHeaderString = SubHeader.HasValue ? $"|0x{SubHeader.Value:X}" : "";
        return $"0x{Header:X}{subHeaderString}";
    }
}