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
    public PacketFieldInfo? DynamicSizeField { get; set; }
    public PacketFieldInfo? DynamicField { get; set; }
    public ImmutableArray<PacketFieldInfo> Fields { get; set; }
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

        var recordParams = node.DescendantNodes().OfType<ParameterSyntax>().ToList();
        Fields = GetFields(semanticModel, symbol.GetMembers().OfType<IFieldSymbol>(), recordParams);
        DynamicSizeField = GetDynamicSizeField(symbol);
        DynamicField = Fields.FirstOrDefault(x => x.HasDynamicLength);
        FixedSize = Fields.Any(x => x.HasDynamicLength) ? null : Fields.Sum(x => x.FieldSize);
        FixedSize++; // Header
        if (SubHeader is not null)
        {
            FixedSize++;
        }

        if (FixedSize is null && DynamicSizeField == null)
        {
            Diagnostics.Add(CreateDiagnostic(node, GeneratorCodes.DYNAMIC_REQUIRES_SIZE_FIELD, GeneratorCodes.DYNAMIC_REQUIRES_SIZE_FIELD_MESSAGE));
        }

        if (FixedSize is null && 
            DynamicSizeField is not null && 
            DynamicField is not null &&
            Fields.IndexOf(DynamicSizeField) > Fields.IndexOf(DynamicField))
        {
            Diagnostics.Add(CreateDiagnostic(node, GeneratorCodes.DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD, GeneratorCodes.DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD_MESSAGE));
        }

        if (Fields.Count(x => x.HasDynamicLength) > 1)
        {
            Diagnostics.Add(CreateDiagnostic(node, GeneratorCodes.DYNAMIC_FIELDS_MAX_ONCE, GeneratorCodes.DYNAMIC_FIELDS_MAX_ONCE_MESSAGE));
        }
    }

    private static Diagnostic CreateDiagnostic(SyntaxNode node, string id, string message)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(
                id,
                "Failed to collect data about packet type",
                message,
                "generators",
                DiagnosticSeverity.Error,
                true),
            node.GetLocation());
    }

    private PacketFieldInfo? GetDynamicSizeField(INamedTypeSymbol symbol)
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

    private static ImmutableArray<PacketFieldInfo> GetFields(SemanticModel model, IEnumerable<IFieldSymbol> fields,
        IEnumerable<ParameterSyntax> recordParams)
    {
        var list = new List<PacketFieldInfo>();
        foreach (var field in fields)
        {
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
            var elementSize = field.Type.GetFullName() == typeof(string).FullName && stringLength is not null
                ? stringLength.Value
                : GetStaticSize(field.Type);
            var info = new PacketFieldInfo
            {
                Name = field.Name,
                Order = order,
                IsArray = isArray,
                ArrayLength = arrayLength,
                IsCustom = isCustomType,
                TypeFullName = isArray ? "System.Array" : field.Type.GetFullName()!,
                IsEnum = isEnum,
                ElementSize = elementSize,
                IsReadonly = field.IsReadOnly,
                IsRecordParameter = false
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

        foreach (var param in recordParams)
        {
            var field = model.GetDeclaredSymbol(param);
            var fixedSizeArrayAttribute = field!.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIXED_SIZE_ARRAY_ATTRIBUTE);
            var orderAttribute = field.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIELD_POSITION_ATTRIBUTE);
            var order = (int?)orderAttribute?.ConstructorArguments.First().Value;
            var isArray = field.Type.BaseType.GetFullName() == typeof(Array).FullName;
            var arrayLength = !isArray
                ? null
                : (int?)fixedSizeArrayAttribute?.ConstructorArguments.First().Value;
            var isCustomType = IsCustomType(field.Type);
            var isEnum = field.Type.BaseType.GetFullName() == typeof(Enum).FullName;
            var info = new PacketFieldInfo
            {
                Name = field.Name,
                Order = order,
                IsArray = isArray,
                ArrayLength = arrayLength,
                IsCustom = isCustomType,
                TypeFullName = isArray ? "System.Array" : field.Type.GetFullName()!,
                IsEnum = isEnum,
                ElementSize = GetStaticSize(field.Type),
                IsReadonly = false,
                IsRecordParameter = false
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
        return fieldType.GetFullNamespace() != "System" && !fieldType.GetFullNamespace()!.StartsWith("System.") &&
               fieldType.TypeKind is not TypeKind.Enum and not TypeKind.Array;
    }

    private static bool IsCustomType(string typeFullName)
    {
        return !typeFullName.StartsWith("System.");
    }

    private static TypeDeclarationSyntax GetTypeDeclaration(ITypeSymbol semanticType)
    {
        return semanticType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .First();
    }
}
