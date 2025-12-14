namespace QuantumCore.Networking;

internal static class GeneratorConstants
{
    internal static string[] SupportedTypesByBitConverter =
        {"Half", "Double", "Single", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64", "Char"};

    internal static string[] NoCastTypes = {"Byte", "SByte"};
    public const string FieldAttributeFullName = "QuantumCore.Networking.FieldAttribute";
    public const string SubPacketAttributeFullName = "QuantumCore.Networking.SubPacketAttribute";
    public const string PacketGeneratorAttributeFullName = "QuantumCore.Networking.PacketGeneratorAttribute";
}
