namespace QuantumCore.Networking;

internal static class GeneratorConstants
{
    internal static string[] SupportedTypesByBitConverter =
    {
        "Half", "Double", "Single", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64", "Char"
    };

    internal static string[] NoCastTypes = {"Byte", "SByte"};
    public const string FIELDATTRIBUTE_FULLNAME = "QuantumCore.Networking.FieldAttribute";
    public const string FIELDATTRIBUTE_POSITION_NAME = "Position";
    public const string FIELDATTRIBUTE_ARRAY_LENGTH_NAME = "ArrayLength";
    public const string SUBPACKETATTRIBUTE_FULLNAME = "QuantumCore.Networking.SubPacketAttribute";
    public const string PACKETGENEREATOR_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.PacketGeneratorAttribute";
    public const string PACKETGENEREATOR_ATTRIBUTE_SUBHEADER = "SubHeader";
    public const string PACKETGENEREATOR_ATTRIBUTE_DYNAMIC = "IsDynamic";
    public const string PACKETGENEREATOR_ATTRIBUTE_DYNAMICSIZE_FIELDNAME = "Size";
    public const string DYNAMICSIZE_FIELD_ATTRIBUTE = "QuantumCore.Networking.DynamicSizeFieldAttribute";
    public const string SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.ServerToClientPacketAttribute";
    public const string CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.ClientToServerPacketAttribute";
}
