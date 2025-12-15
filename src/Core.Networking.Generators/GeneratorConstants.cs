namespace QuantumCore.Networking;

internal static class GeneratorConstants
{
    internal static readonly string[] SupportedTypesByBitConverter =
        ["Half", "Double", "Single", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64", "Char"];

    internal static readonly string[] NoCastTypes = ["Byte", "SByte"];
    public const string FIELD_ATTRIBUTE_FULL_NAME = "QuantumCore.Networking.FieldAttribute";
    public const string SUB_PACKET_ATTRIBUTE_FULL_NAME = "QuantumCore.Networking.SubPacketAttribute";
    public const string PACKET_GENERATOR_ATTRIBUTE_FULL_NAME = "QuantumCore.Networking.PacketGeneratorAttribute";
}
