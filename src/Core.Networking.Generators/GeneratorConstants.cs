using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal static class GeneratorConstants
{
    internal static readonly string[] SupportedTypesByBitConverter =
    [
        "System.Half",
        "System.Double",
        "System.Single",
        "System.Int16",
        "System.Int32",
        "System.Int64",
        "System.UInt16",
        "System.UInt32",
        "System.UInt64",
        "System.Char"
    ];

    internal static readonly string[] ConvertTypes = ["System.Boolean"];
    internal static readonly string[] NoCastTypes = ["System.Byte", "System.SByte"];
    public const string HAS_SEQUENCE_NAME = "HasSequence";

    public const string PACKET_CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME =
        "QuantumCore.Networking.ClientToServerPacketAttribute";

    public const string PACKET_SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME =
        "QuantumCore.Networking.ServerToClientPacketAttribute";

    public const string STRING_LENGTH_ATTRIBUTE_FULLNAME =
        "QuantumCore.Networking.FixedSizeStringAttribute";

    public const string ARRAY_LENGTH_ATTRIBUTE_FULLNAME =
        "QuantumCore.Networking.FixedSizeArrayAttribute";

    public const string? SIZE_FIELD_ATTRIBUTE = "QuantumCore.Networking.DynamicSizeForAttribute";

    public const string SUB_HEADER_FIELD_NAME = "SubHeader";


    internal static int GetSizeOfPrimitiveType(string name)
    {
        return name switch
        {
            "System.Int64" or "System.UInt64" or "System.Double" => 8,
            "System.Int32" or "System.UInt32" or "System.Single" => 4,
            "System.Int16" or "System.UInt16" or "System.Half" => 2,
            "System.Byte" or "System.SByte" or "System.Boolean" => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(name), $"Don't know the size of {name}")
        };
    }

    public static string GeneratePackageHandler(string assemblyName, ImmutableArray<SerializerTypeInfo> typeInfos,
        INamedTypeSymbol attributeType)
    {
        var constructorParameters = GetConstructorParameters(typeInfos);
        var fields = GetFields(typeInfos);
        var handleSwitchCases = GetHandleSwitchCases(typeInfos, attributeType);
        var typeInfoSwitchCases = GetTypeInfoSwitchCases(typeInfos, attributeType);
        var subHeaderDefinitionCases = GetSubHeaderDefinitionCases(typeInfos, attributeType);
        var fieldAssignments = GetFieldAssignments(typeInfos);
        var namespaces = GetNamespaces(typeInfos);
        return $$"""
                 using System;
                 using Microsoft.Extensions.Logging;{{namespaces}}

                 namespace QuantumCore.Networking;

                 public class {{assemblyName}}PacketHandlerManager : IPacketHandlerManager
                 {
                     private readonly ILogger<PacketHandlerManager> _logger;{{fields}}
                 
                     public PacketHandlerManager(ILogger<PacketHandlerManager> logger{{constructorParameters}})
                     {
                         _logger = logger;{{fieldAssignments}}
                     }
                 
                     public void HandlePacket(ReadOnlySpan<byte> buffer, byte header, byte? subHeader)
                     {
                         try {
                             switch (header) {{{handleSwitchCases}}
                                 default:
                                     _logger.LogError("No packet handler found for packet with header {Header}{SubHeader}", $"0x{header:X2}", $"0x{subHeader:X2}");
                                     break;
                             }
                         }
                         catch (Exception e)
                         {
                             _logger.LogError(e, "Failed to execute packet handler for header {Header}{SubHeader}", $"0x{header:X2}", $"0x{subHeader:X2}");
                         }
                     }
                     
                 
                     public bool TryGetPacketInfo(in byte header, in byte? subHeader, out PacketInfo packetInfo)
                     {
                         switch (header) {{{typeInfoSwitchCases}}
                             default:
                                 packetInfo = new PacketInfo();
                                 return false;
                         }
                     }
                     
                     public bool IsSubPacketDefinition(in byte header)
                     {
                         switch (header) {{{subHeaderDefinitionCases}}
                             default:
                                 return false;
                         }
                     }
                 }
                 """;
    }

    private static string GetTypeInfoSwitchCases(ImmutableArray<SerializerTypeInfo> typeInfos,
        INamedTypeSymbol attributeType)
    {
        var switches = typeInfos.Select(x =>
        {
            var packetAttr = x.Attributes
                .First(x => x.AttributeClass.Equals(attributeType, SymbolEqualityComparer.Default));
            var subHeaderValue = GetSubPacketValue(packetAttr);
            var subHeaderString = subHeaderValue.HasValue
                ? $" when subHeader == 0x{subHeaderValue:X2}"
                : "";
            var headerValue = GetHeaderValue(packetAttr);
            var hasStaticSize = GetStaticSize(x) is not null;
            var hasSequence = GetHasSequence(packetAttr);
            return
                $"""
                                 case {headerValue}{subHeaderString}:
                                     packetInfo = new PacketInfo({hasStaticSize.ToString().ToLower()}, {hasSequence.ToString().ToLower()});
                                     break;
                 """;
        }).ToArray();
        if (switches.Length > 0)
        {
            return "\n" + string.Join("\n", switches);
        }

        return "";
    }

    private static string GetSubHeaderDefinitionCases(ImmutableArray<SerializerTypeInfo> typeInfos,
        INamedTypeSymbol attributeType)
    {
        var switches = typeInfos
            .GroupBy(x =>
                GetHeaderValue(x.Attributes.First(attr =>
                    attr.AttributeClass.Equals(attributeType, SymbolEqualityComparer.Default))))
            .Select(pair =>
            {
                var hasAnySubHeader = pair.Any(x =>
                {
                    var packetAttr = x.Attributes
                        .First(x => x.AttributeClass.Equals(attributeType, SymbolEqualityComparer.Default));
                    return GetSubPacketValue(packetAttr) is not null;
                });
                return
                    $"""
                                     case 0x{pair.Key:X2}:
                                         return {hasAnySubHeader.ToString().ToLowerInvariant()};
                     """;
            })
            .ToArray();
        if (switches.Length > 0)
        {
            return "\n" + string.Join("\n", switches);
        }

        return "";
    }

    private static int? GetStaticSize(SerializerTypeInfo info)
    {
        return info.Fields.Any(x => x.HasDynamicLength)
            ? null
            : info.Fields.Sum(x => x.FieldSize);
    }

    private static string GetNamespaces(ImmutableArray<SerializerTypeInfo> typeInfos)
    {
        var namespaces = typeInfos
            .Select(x => $"using {x.Namespace};")
            .Distinct()
            .ToArray();

        return namespaces.Length > 0
            ? "\n" + string.Join("\n", namespaces)
            : "";
    }

    private static string GetConstructorParameters(ImmutableArray<SerializerTypeInfo> typeInfos)
    {
        var lines = typeInfos.Select(x =>
        {
            return $"        {x.Name}Handler {x.Name[0].ToString().ToLower()}{x.Name.Substring(1)}Handler";
        }).ToArray();
        if (lines.Length > 0)
        {
            return ",\n" + string.Join(",\n", lines);
        }
        else
        {
            return string.Join(",\n", lines);
        }
    }

    private static string GetHandleSwitchCases(ImmutableArray<SerializerTypeInfo> typeInfos,
        INamedTypeSymbol attributeType)
    {
        var switches = typeInfos
            .Select(info =>
            {
                var packetAttr = info.Attributes
                    .First(x => x.AttributeClass!.Equals(attributeType, SymbolEqualityComparer.Default));
                return
                    $"""
                                     case {GetHeaderValue(packetAttr)}:
                                         {GetHandlerFieldName(info.Name)}.Execute(new {info.Name}(buffer));
                                         break;
                     """;
            })
            .ToArray();
        if (switches.Length > 0)
        {
            return "\n" + string.Join("\n", switches);
        }

        return "";
    }

    private static string GetFieldAssignments(ImmutableArray<SerializerTypeInfo> typeInfos)
    {
        var list = ImmutableArrayExtensions.Select(typeInfos,
            x =>
            {
                return
                    $"        {GetHandlerFieldName(x.Name)} = {x.Name[0].ToString().ToLower()}{x.Name.Substring(1)}Handler;";
            }).ToArray();
        return list.Length > 0
            ? "\n" + string.Join("\n", list)
            : "";
    }

    private static string GetHandlerFieldName(string name)
    {
        return $"_{name[0].ToString().ToLower()}{name.Substring(1)}Handler";
    }

    private static string GetFields(ImmutableArray<SerializerTypeInfo> typeInfos)
    {
        var fields = typeInfos.Select(x =>
        {
            return $"    private readonly {x.Name}Handler {GetHandlerFieldName(x.Name)};";
        }).ToArray();
        if (fields.Length > 0)
        {
            return "\n" + string.Join("\n", fields);
        }
        else
        {
            return "";
        }
    }

    public static PacketInfo GetPacketInfo(SerializerTypeInfo info, INamedTypeSymbol attributeType)
    {
        var packetAttr = info.Attributes
            .First(x => x.AttributeClass.Equals(attributeType, SymbolEqualityComparer.Default));
        var header = GetHeaderValue(packetAttr);
        var subHeader = GetSubPacketValue(packetAttr);
        var hasSequence = GetHasSequence(packetAttr);
        return new PacketInfo(
            header,
            subHeader,
            hasSequence
        );
    }

    public static string GetTypeModifiers(MemberDeclarationSyntax type)
    {
        return string.Join(" ", type.Modifiers.Select(x => x.Text));
    }

    public static byte? GetSubPacketValue(AttributeData attr)
    {
        return attr.ConstructorArguments.Length == 2
            ? (byte) attr.ConstructorArguments[1].Value!
            : null;
    }

    public static byte GetHeaderValue(AttributeData attr)
    {
        return (byte) attr.ConstructorArguments[0].Value!;
    }

    public static bool GetHasSequence(AttributeData attr)
    {
        if (!attr.NamedArguments.Any(x => x.Key == HAS_SEQUENCE_NAME)) return false;

        var typedConstant = attr.NamedArguments.First(x => x.Key == HAS_SEQUENCE_NAME).Value;
        return (bool) typedConstant.Value!;
    }

    public static bool IsReadonlyRefSpan(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        return syntaxNode is StructDeclarationSyntax structSyntax &&
               structSyntax.Modifiers.Any(x => x.Text == "readonly") &&
               structSyntax.Modifiers.Any(x => x.Text == "ref") &&
               IsPartial(structSyntax);
    }

    public static bool IsClass(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax classSyntax &&
               IsPartial(classSyntax);
    }

    public static bool IsPartial(TypeDeclarationSyntax declaration)
    {
        return declaration.Modifiers.Any(x => x.Text == "partial");
    }

    public static AttributeData? GetServerToClientAttribute(SerializerTypeInfo info)
    {
        return info.Attributes
            .First(x => x.AttributeClass!.Name is "ServerToClientPacketAttribute" or "ServerToClientPacket");
    }

    public static AttributeData? GetClientToServerAttribute(SerializerTypeInfo info)
    {
        return info.Attributes
            .FirstOrDefault(x => x.AttributeClass!.Name is "ClientToServerPacketAttribute" or "ClientToServerPacket");
    }

    public static SerializerTypeInfo GetTypeInfo(GeneratorAttributeSyntaxContext arg1, CancellationToken arg2)
    {
        return new SerializerTypeInfo(arg1.SemanticModel, (TypeDeclarationSyntax) arg1.TargetNode);
    }

    internal static bool IsCustomType(ITypeSymbol fieldType)
    {
        return !fieldType.GetFullName()!.StartsWith("System.") &&
               fieldType.TypeKind is not TypeKind.Enum and not TypeKind.Array;
    }

    internal static bool IsCustomType(string typeFullName)
    {
        return !typeFullName.StartsWith("System.");
    }

    internal static FieldData BuildFieldData(SemanticModel semanticModel, IFieldSymbol field)
    {
        var isReadonly = field.IsReadOnly;
        var isArray = false;
        var fieldType = field.Type;
        string? elementTypeFullName = null;
        FieldData[]? elementTypeFields = null;

        if (field.Type.TypeKind is TypeKind.Array)
        {
            isArray = true;
            var elementType = ((IArrayTypeSymbol) field.Type).ElementType;
            elementTypeFullName = elementType.GetFullName()!;
            if (IsCustomType(elementType))
            {
                elementTypeFields = elementType
                    .GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                    .Select(x => BuildFieldData(semanticModel, x))
                    .ToArray();
            }
        }

        var enumType = isArray ? null : fieldType;
        var isEnum = enumType?.TypeKind is TypeKind.Enum;
        string? sizeFieldName = null;

        if (isEnum)
        {
            elementTypeFullName = ((INamedTypeSymbol) fieldType).EnumUnderlyingType.GetFullName();
        }

        if (fieldType.GetFullName() == "System.String" || isArray)
        {
            var dynamicSizeField = fieldType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Select(x => (x, x.GetAttributes()))
                .Where(pair =>
                    pair.Item2.Any(attr => attr.AttributeClass.GetFullName() == SIZE_FIELD_ATTRIBUTE &&
                                           attr.ConstructorArguments[0].Value!.ToString() == field.Name))
                .Select(x => x.x)
                .FirstOrDefault();
            if (dynamicSizeField is not null)
            {
                sizeFieldName = dynamicSizeField.Name;
            }
        }

        int? stringLength = null;
        var stringLengthAttr = field.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass.GetFullName() == STRING_LENGTH_ATTRIBUTE_FULLNAME);
        if (fieldType.GetFullName() == "System.String" && stringLengthAttr is not null)
        {
            stringLength = (int) stringLengthAttr.ConstructorArguments[0].Value!;
        }


        int? arrayLength = null;
        var arrayLengthAttr = field.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass.GetFullName() == ARRAY_LENGTH_ATTRIBUTE_FULLNAME);
        if (isArray && arrayLengthAttr is not null)
        {
            arrayLength = (int) arrayLengthAttr.ConstructorArguments[0].Value!;
        }

        var elementSize = GetStaticSize(semanticModel, fieldType, arrayLength);
        if ((isArray || fieldType.GetFullName() == "System.String") &&
            stringLength is not null)
        {
            elementSize = stringLength.Value;
        }

        if ((fieldType.GetFullName() == "System.String" &&
             stringLength is null &&
             sizeFieldName is null
            ) || (isArray &&
                  sizeFieldName is null &&
                  arrayLength is null))
        {
            throw new DiagnosticException("QCX000002",
                "String or array must have a defined a static length either via FieldAttribute or an array constructor as default value. Dynamic fields must have a field that refers to it's size like \"public uint Size => Message.Length;\"",
                field.Locations.First());
        }

        FieldData[]? subFields = null;
        var isCustomType = IsCustomType(fieldType);
        if (isCustomType)
        {
            subFields = fieldType
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Select(x => BuildFieldData(semanticModel, x))
                .ToArray();
        }

        return new FieldData
        {
            FieldName = field.Name,
            TypeFullName = fieldType.GetFullName()!,
            IsArray = isArray,
            IsEnum = isEnum,
            IsCustom = isCustomType,
            ArrayLength = arrayLength,
            ElementSize = elementSize,
            ElementTypeFullName = elementTypeFullName,
            ElementTypeFields = elementTypeFields,
            IsReadonly = isReadonly,
            SizeFieldName = sizeFieldName,
            SubFields = subFields
        };
    }

    internal static FieldData BuildFieldData(SemanticModel semanticModel, IPropertySymbol property)
    {
        var isReadonly = property.SetMethod is null;
        var isArray = false;
        var fieldType = property.Type;
        string? elementTypeFullName = null;
        FieldData[]? elementTypeFields = null;

        if (property.Type.TypeKind is TypeKind.Array)
        {
            isArray = true;
            var elementType = ((IArrayTypeSymbol) property.Type).ElementType;
            elementTypeFullName = elementType.GetFullName()!;
            if (IsCustomType(elementType))
            {
                elementTypeFields = elementType
                    .GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                    .Select(x => BuildFieldData(semanticModel, x))
                    .ToArray();
            }
        }

        var enumType = isArray ? null : fieldType;
        var isEnum = enumType?.TypeKind is TypeKind.Enum;
        string? sizeFieldName = null;

        if (isEnum)
        {
            elementTypeFullName = ((INamedTypeSymbol) fieldType).EnumUnderlyingType.GetFullName();
        }

        if (fieldType.GetFullName() == "System.String" || isArray)
        {
            var dynamicSizeField = fieldType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Select(x => (x, x.GetAttributes()))
                .Where(pair =>
                    pair.Item2.Any(attr => attr.AttributeClass.GetFullName() == SIZE_FIELD_ATTRIBUTE &&
                                           attr.ConstructorArguments[0].Value!.ToString() == property.Name))
                .Select(x => x.x)
                .FirstOrDefault();
            if (dynamicSizeField is not null)
            {
                sizeFieldName = dynamicSizeField.Name;
            }
        }

        int? stringLength = null;
        var stringLengthAttr = property.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass.GetFullName() == STRING_LENGTH_ATTRIBUTE_FULLNAME);
        if (fieldType.GetFullName() == "System.String" && stringLengthAttr is not null)
        {
            stringLength = (int) stringLengthAttr.ConstructorArguments[0].Value!;
        }


        int? arrayLength = null;
        var arrayLengthAttr = property.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass.GetFullName() == ARRAY_LENGTH_ATTRIBUTE_FULLNAME);
        if (isArray && arrayLengthAttr is not null)
        {
            arrayLength = (int) arrayLengthAttr.ConstructorArguments[0].Value!;
        }

        var elementSize = GetStaticSize(semanticModel, fieldType, arrayLength);
        if ((isArray || fieldType.GetFullName() == "System.String") &&
            stringLength is not null)
        {
            elementSize = stringLength.Value;
        }

        if ((fieldType.GetFullName() == "System.String" &&
             stringLength is null &&
             sizeFieldName is null
            ) || (isArray &&
                  sizeFieldName is null &&
                  arrayLength is null))
        {
            throw new DiagnosticException("QCX000002",
                "String or array must have a defined a static length either via FieldAttribute or an array constructor as default value. Dynamic fields must have a field that refers to it's size like \"public uint Size => Message.Length;\"",
                property.Locations.First());
        }

        FieldData[]? subFields = null;
        var isCustomType = IsCustomType(fieldType);
        if (isCustomType)
        {
            subFields = fieldType
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Select(x => BuildFieldData(semanticModel, x))
                .ToArray();
        }

        return new FieldData
        {
            FieldName = property.Name,
            TypeFullName = fieldType.GetFullName()!,
            IsArray = isArray,
            IsEnum = isEnum,
            IsCustom = isCustomType,
            ArrayLength = arrayLength,
            ElementSize = elementSize,
            ElementTypeFullName = elementTypeFullName,
            ElementTypeFields = elementTypeFields,
            IsReadonly = isReadonly,
            SizeFieldName = sizeFieldName,
            SubFields = subFields
        };
    }

    internal static int GetStaticSize(SemanticModel semanticModel, ITypeSymbol semanticType,
        int? arrayLength = null)
    {
        var typeName = semanticType.Name;

        switch (typeName)
        {
            case "Int64":
            case "UInt64":
            case "Double":
                return 8;
            case "Int32":
            case "UInt32":
            case "Single":
                return 4;
            case "Byte":
            case "SByte":
            case "Boolean":
                return 1;
            case "Int16":
            case "UInt16":
                return 2;
            case "String":
                // dynamic size - does not contribute to static size
                return 0;
            default:
                if (IsCustomType(semanticType))
                {
                    // probably a custom type
                    return SerializerTypeInfo.GetStaticSizeOfType(
                        new SerializerTypeInfo(semanticModel, (INamedTypeSymbol) semanticType)
                            .Fields);
                }
                else if (semanticType is IArrayTypeSymbol arr)
                {
                    if (arrayLength is null)
                    {
                        // may have dynamic size
                        return GetStaticSize(semanticModel, arr.ElementType);
                    }
                    else
                    {
                        return GetStaticSize(semanticModel, arr.ElementType);
                    }
                }
                else if (semanticType.TypeKind is TypeKind.Enum && semanticType is INamedTypeSymbol namedTypeSymbol)
                {
                    return GetStaticSize(semanticModel, namedTypeSymbol.EnumUnderlyingType!);
                }

                throw new NotImplementedException($"Don't know how to handle {semanticType.Name}");
        }
    }
}