namespace QuantumCore.Networking;

internal static class GeneratorConstants
{
    internal static string[] SupportedTypesByBitConverter =
        ["Half", "Double", "Single", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64", "Char"];
    internal static string[] ConvertTypes = ["Boolean"];
    internal static string[] NoCastTypes = ["Byte", "SByte"];
    public const string FIELDATTRIBUTE_FULLNAME = "QuantumCore.Networking.FieldAttribute";
    public const string SUBPACKETATTRIBUTE_FULLNAME = "QuantumCore.Networking.SubPacketAttribute";
    public const string PACKETGENEREATOR_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.PacketGeneratorAttribute";

    internal static int GetSizeOfPrimitiveType(string name)
    {
        return name switch
        {
            "Int64" or "UInt64" or "Double" => 8,
            "Int32" or "UInt32" or "Single" => 4,
            "Int16" or "UInt16" or "Half" => 2,
            "Byte" or "SByte" or "Boolean" => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(name), $"Don't know the size of {name}")
        };
    }
}
