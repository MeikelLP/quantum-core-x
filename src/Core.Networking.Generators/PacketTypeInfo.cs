using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

public class PacketTypeInfo
{
    public byte Header { get; set; }
    public byte? SubHeader { get; set; }
    public int? FixedSize { get; set; }
    public bool IsClientToServer { get; set; }
    public bool IsServerToClient { get; set; }
    public string Namespace { get; set; }
    public string Name { get; set; }
    public PacketFieldInfo2? DynamicSizeField { get; set; }
    public PacketFieldInfo2? DynamicField { get; set; }
    public ImmutableArray<PacketFieldInfo2> Fields { get; set; }
    public List<Diagnostic> Diagnostics { get; set; } = [];


    internal PacketTypeInfo(string ns, string name)
    {
        Namespace = ns;
        Name = name;
    }

    public PacketTypeInfo(bool serverToClient, INamedTypeSymbol symbol, SyntaxNode node, SemanticModel semanticModel)
    {
        Name = symbol.Name;
        Namespace = symbol.GetFullNamespace()!;
        AttributeData packetAttribute;
        if (serverToClient)
        {
            packetAttribute = symbol
                .GetAttributes()
                .First(x => x.AttributeClass?.GetFullName() == GeneratorConstants.SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME);
            IsServerToClient = true;
        }
        else
        {
            packetAttribute = symbol
                .GetAttributes()
                .First(x => x.AttributeClass?.GetFullName() == GeneratorConstants.CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME);
            IsClientToServer = true;
        }

        Header = (byte)packetAttribute.ConstructorArguments[0].Value!;
        SubHeader = (byte?)packetAttribute.ConstructorArguments.ElementAtOrDefault(1).Value;

        Fields = GetFields(symbol, SubHeader);
        DynamicSizeField = GetDynamicSizeField(symbol);
        DynamicField = Fields.FirstOrDefault(x => x.HasDynamicLength);
        FixedSize = Fields.Any(x => x.HasDynamicLength) ? null : Fields.Sum(x => x.FieldSize);
        FixedSize++; // Header

        ReportDiagnostics(node);
    }

    private void ReportDiagnostics(SyntaxNode node)
    {
        var fieldNodes = GetFieldNodes(node);
        if (FixedSize is null && DynamicSizeField == null)
        {
            var field = Fields.First(x => x.HasDynamicLength);
            var location = fieldNodes[field.Name];
            Diagnostics.Add(CreateDiagnostic(location, GeneratorCodes.DYNAMIC_REQUIRES_SIZE_FIELD, GeneratorCodes.DYNAMIC_REQUIRES_SIZE_FIELD_MESSAGE));
        }

        if (FixedSize is null && 
            DynamicSizeField is not null && 
            DynamicField is not null &&
            Fields.IndexOf(DynamicSizeField) > Fields.IndexOf(DynamicField))
        {
            var location = fieldNodes[DynamicSizeField.Name];
            Diagnostics.Add(CreateDiagnostic(location, GeneratorCodes.DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD, GeneratorCodes.DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD_MESSAGE));
        }

        var exceedingDynamicFields = Fields.Where((_, i) => i > 1);
        foreach (var field in exceedingDynamicFields)
        {
            var location = fieldNodes[field.Name];
            Diagnostics.Add(CreateDiagnostic(location, GeneratorCodes.DYNAMIC_FIELDS_MAX_ONCE, GeneratorCodes.DYNAMIC_FIELDS_MAX_ONCE_MESSAGE));
        }

        foreach (var field in Fields.Where(x => x.IsReadonly))
        {
            var location = fieldNodes[field.Name];
            Diagnostics.Add(CreateDiagnostic(location, GeneratorCodes.READONLY_NOT_SUPPORTED, GeneratorCodes.READONLY_NOT_SUPPORTED_MESSAGE));
        }

        foreach (var field in Fields.Where(x => x.TypeFullName == $"{Namespace}.{Name}"))
        {
            var location = fieldNodes[field.Name];
            Diagnostics.Add(CreateDiagnostic(location, GeneratorCodes.SELF_REFERENCE_LOOP, GeneratorCodes.SELF_REFERENCE_LOOP_MESSAGE));
        }
    }

    private static Dictionary<string, Location> GetFieldNodes(SyntaxNode node)
    {
        return node.DescendantNodes()
            .Where(x => x is FieldDeclarationSyntax)
            .Select(x =>x.DescendantNodes().OfType<VariableDeclarationSyntax>().First().Variables.First().Identifier)
            .ToDictionary(x => x.ValueText, x => x.GetLocation());
    }

    private static Diagnostic CreateDiagnostic(Location location, string id, string message)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                id,
                "Failed to collect data about packet type",
                message,
                "generators",
                DiagnosticSeverity.Error,
                true),
            location);
    }

    private PacketFieldInfo2? GetDynamicSizeField(INamedTypeSymbol symbol)
    {
        if (FixedSize is not null) return null;
        var fieldByConvention = Fields.FirstOrDefault(x =>
            x.Name == GeneratorConstants.PACKETGENEREATOR_ATTRIBUTE_DYNAMICSIZE_FIELDNAME);
        var fieldSymbolWithAttribute = symbol.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(x =>
            x.GetAttributes().Any(a =>
                a.AttributeClass.GetFullName() == GeneratorConstants.DYNAMICSIZE_FIELD_ATTRIBUTE));
        var fieldByAttribute = Fields.FirstOrDefault(x => x.Name == fieldSymbolWithAttribute?.Name);
        return fieldByAttribute ?? fieldByConvention;
    }

    private ImmutableArray<PacketFieldInfo2> GetFields(ITypeSymbol symbol, byte? subHeader)
    {
        var fields = symbol.GetMembers().OfType<IFieldSymbol>();
        var fieldNodes = GetFieldNodes(symbol.DeclaringSyntaxReferences.First().GetSyntax());
        var list = new List<PacketFieldInfo2>();
        if (subHeader is not null)
        {
            list.Add(new PacketFieldInfo2
            {
                Name = GeneratorConstants.SUBHEADER_RESERVED_NAME,
                ElementSize = 1,
                TypeFullName = typeof(byte).FullName!,
                ConstantValue = subHeader
            });
        }
        foreach (var field in fields)
        {
            if (SymbolEqualityComparer.Default.Equals(field.Type, symbol))
            {
                Diagnostics.Add(CreateDiagnostic(fieldNodes[field.Name], GeneratorCodes.SELF_REFERENCE_LOOP, GeneratorCodes.SELF_REFERENCE_LOOP_MESSAGE));
                continue;
            }
            if (field.Name == GeneratorConstants.SUBHEADER_RESERVED_NAME)
            {
                Diagnostics.Add(CreateDiagnostic(fieldNodes[field.Name], GeneratorCodes.SUBHEADER_NAME_RESERVED, GeneratorCodes.SUBHEADER_NAME_RESERVED_MESSAGE));
                continue;
            }
            
            var fixedSizeArrayAttribute = field!.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIXED_SIZE_ARRAY_ATTRIBUTE);
            var fixedSizeStringAttribute = field!.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIXED_SIZE_STRING_ATTRIBUTE);
            var orderAttribute = field.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIELD_POSITION_ATTRIBUTE);
            var order = (int?)orderAttribute?.ConstructorArguments.First().Value;
            var isArray = field.Type.BaseType.GetFullName() == typeof(Array).FullName;
            var arrayLength = !isArray
                ? null
                : (int?)fixedSizeArrayAttribute?.ConstructorArguments.First().Value;
            var stringLength = (int?)fixedSizeStringAttribute?.ConstructorArguments.First().Value;
            var isCustomType = IsCustomType(field.Type);
            var isEnum = field.Type.BaseType.GetFullName() == typeof(Enum).FullName;
            var elementType = true switch
            {
                true when isArray => ((IArrayTypeSymbol)field.Type).ElementType,
                true when isEnum => ((INamedTypeSymbol)field.Type).EnumUnderlyingType,
                _ => null
            };
            var subFields = isCustomType ? GetFields(field.Type, null) : [];
            var elementSize = field.Type.GetFullName() == typeof(string).FullName && stringLength is not null
                ? stringLength.Value
                : isCustomType
                    ? subFields.Sum(x => x.FieldSize)
                    : GetStaticSize(field.Type);
            var elementTypeFullName = elementType.GetFullName()!;
            var info = new PacketFieldInfo2
            {
                Name = field.Name,
                Order = order,
                IsArray = isArray,
                ArrayLength = arrayLength,
                IsCustom = isCustomType,
                TypeFullName = isArray ? typeof(Array).FullName! : field.Type.GetFullName()!,
                ElementTypeFullName = elementTypeFullName,
                IsEnum = isEnum,
                ElementSize = elementSize,
                IsReadonly = field.IsReadOnly,
                Fields = subFields
            };
            if (order.HasValue)
            {
                list.Insert(order.Value, info);
            }
            else
            {
                list.Add(info);
            }
        }

        return [..list.OrderBy(x => x.Order, new FieldOrderComparer())];
    }

    private class FieldOrderComparer : IComparer<int?>
    {
        public int Compare(int? x, int? y)
        {
            if (x == null && y == null) return 0;
            if (x is not null && y is null) return -1;
            if (y is null && y is not null) return 1;
            return x!.Value.CompareTo(y!.Value);
        }
    }

    /// <summary>
    /// May return 0 if type is dynamic
    /// </summary>
    private static int GetStaticSize(ITypeSymbol type)
    {
        var typeFullName = type.GetFullName()!;
        switch (typeFullName)
        {
            case "System.Int64":
            case "System.UInt64":
            case "System.Double":
                return 8;
            case "System.Int32":
            case "System.UInt32":
            case "System.Single":
                return 4;
            case "System.Byte":
            case "System.SByte":
            case "System.Boolean":
                return 1;
            case "System.Int16":
            case "System.UInt16":
                return 2;
            case "System.String":
                // dynamic size - does not contribute to static size
                return 0;
        }

        if (type.TypeKind == TypeKind.Enum)
        {
            return GetStaticSize(((INamedTypeSymbol)type).EnumUnderlyingType!);
        }

        if (type.TypeKind == TypeKind.Array)
        {
            return GetStaticSize(((IArrayTypeSymbol)type).ElementType);
        }

        throw new NotImplementedException("Don't know how to handle type: " + typeFullName);
    }

    private static bool IsCustomType(ITypeSymbol fieldType)
    {
        return (fieldType.GetFullNamespace() != "System" && !fieldType.GetFullNamespace()!.StartsWith("System.") &&
               fieldType.TypeKind is not TypeKind.Enum and not TypeKind.Array) || 
               (fieldType.Kind is SymbolKind.ArrayType && IsCustomType(((IArrayTypeSymbol)fieldType).ElementType));
    }

    private static TypeDeclarationSyntax GetTypeDeclaration(ITypeSymbol semanticType)
    {
        return semanticType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .First();
    }
}
