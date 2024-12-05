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
    public const string PACKETGENEREATOR_ATTRIBUTE_DYNAMICSIZE_FIELDNAME = "Size";
    public const string DYNAMICSIZE_FIELD_ATTRIBUTE = "QuantumCore.Networking.DynamicSizeFieldAttribute";
    public const string FIXED_SIZE_ARRAY_ATTRIBUTE = "QuantumCore.Networking.FixedSizeArrayAttribute";
    public const string FIXED_SIZE_STRING_ATTRIBUTE = "QuantumCore.Networking.FixedSizeStringAttribute";
    public const string FIELD_POSITION_ATTRIBUTE = "QuantumCore.Networking.FieldOrderAttribute";
    public const string SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.ServerToClientPacketAttribute";
    public const string CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME = "QuantumCore.Networking.ClientToServerPacketAttribute";
}

internal static class GeneratorCodes
{
    public const string DYNAMIC_REQUIRES_SIZE_FIELD = "QCX000006";
    public const string DYNAMIC_REQUIRES_SIZE_FIELD_MESSAGE = "Dynamic packet types require a size field to be defined. Either by convention with the name \"Size\" or with the attribute [DynamicSizeFieldAttribute]";
    public const string DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD = "QCX000007";
    public const string DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD_MESSAGE = "The dynamic size field must be ordered before the field with dynamic size";
    public const string DYNAMIC_FIELDS_MAX_ONCE = "QCX000008";
    public const string DYNAMIC_FIELDS_MAX_ONCE_MESSAGE = "Only one dynamic field is supported";
}
