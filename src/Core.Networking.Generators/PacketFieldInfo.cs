using System.Collections.Immutable;

namespace QuantumCore.Networking;

public class PacketFieldInfo
{
    public string Name { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }
    public int? Order { get; set; }
    public string TypeFullName { get; set; } = "";

    public int FieldSize => HasDynamicLength
        ? 0
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => (TypeFullName == typeof(string).FullName && ElementSize == 0) ||
                                    (IsArray && ArrayLength == null);

    public string? SizeFieldName { get; set; }
    public bool IsCustom { get; set; }
    public bool IsRecordParameter { get; set; }
    public bool IsReadonly { get; set; }
}

public class PacketFieldInfo2
{
    public string Name { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }

    /// <summary>
    /// Overriden order in field list if any
    /// </summary>
    public int? Order { get; set; }

    public string TypeFullName { get; set; } = "";

    /// <summary>
    /// Complete size of the field. Is multiplied by array length if applicable
    /// </summary>
    public int FieldSize => HasDynamicLength
        ? 0
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => (TypeFullName == typeof(string).FullName && ElementSize == 0) ||
                                    (IsArray && ArrayLength == null);

    /// <summary>
    /// Is non-primitive types and must be scanned for sub properties
    /// </summary>
    public bool IsCustom { get; set; }

    public bool IsReadonly { get; set; }
    public ImmutableArray<PacketFieldInfo2> Fields { get; set; } = [];

    /// <summary>
    /// Used primarily by SubHeader
    /// </summary>
    public object? ConstantValue { get; set; }

    /// <summary>
    /// Underlying type of array or enum
    /// </summary>
    public string? ElementTypeFullName { get; set; }
}
