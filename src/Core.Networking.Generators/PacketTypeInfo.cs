using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

public class PacketTypeInfo
{
    public byte Header { get; set; }
    public byte? SubHeader { get; set; }
    public bool HasDynamicLength { get; set; }
    public bool IsClientToServer { get; set; }
    public bool IsServerToClient { get; set; }
    public string Namespace { get; set; }
    public string Name { get; set; }
    public PacketFieldInfo? DynamicSizeField { get; set; }
    public ImmutableArray<PacketFieldInfo> Fields { get; set; }

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
        HasDynamicLength = (bool?)packetAttribute.NamedArguments
            .FirstOrDefault(x => x.Key == GeneratorConstants.PACKETGENEREATOR_ATTRIBUTE_DYNAMIC).Value.Value == true;

        var recordParams = node.DescendantNodes().OfType<ParameterSyntax>().ToList();
        Fields = GetFields(semanticModel, symbol.GetMembers().OfType<IFieldSymbol>(), recordParams);
        DynamicSizeField = GetDynamicSizeField(symbol);
    }

    private PacketFieldInfo? GetDynamicSizeField(INamedTypeSymbol symbol)
    {
        if (!HasDynamicLength) return null;
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
            var fieldAttribute = field.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIELDATTRIBUTE_FULLNAME);
            var order = (int?)fieldAttribute?.NamedArguments
                .FirstOrDefault(x => x.Key == GeneratorConstants.FIELDATTRIBUTE_POSITION_NAME).Value.Value;
            var isArray = field.Type.BaseType.GetFullName() == typeof(Array).FullName;
            var arrayLength = !isArray
                ? null
                : (int?)fieldAttribute?.NamedArguments
                    .FirstOrDefault(x => x.Key == GeneratorConstants.FIELDATTRIBUTE_ARRAY_LENGTH_NAME).Value.Value;
            var isCustomType = field.Type.GetFullNamespace() != "System" &&
                               !field.Type.GetFullNamespace()!.StartsWith("System.");
            var isEnum = field.Type.BaseType.GetFullName() == typeof(Enum).FullName;
            var info = new PacketFieldInfo
            {
                Name = field.Name,
                Order = order,
                IsArray = isArray,
                ArrayLength = arrayLength,
                IsCustom = isCustomType,
                TypeFullName = field.Type.GetFullName()!,
                IsEnum = isEnum,
                ElementSize = GetStaticSize(field.Type.GetFullName()!),
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
            var fieldAttribute = field!.GetAttributes().FirstOrDefault(x =>
                x.AttributeClass.GetFullName() == GeneratorConstants.FIELDATTRIBUTE_FULLNAME);
            var order = (int?)fieldAttribute?.NamedArguments
                .FirstOrDefault(x => x.Key == GeneratorConstants.FIELDATTRIBUTE_POSITION_NAME).Value.Value;
            var isArray = field.Type.BaseType.GetFullName() == typeof(Array).FullName;
            var arrayLength = !isArray
                ? null
                : (int?)fieldAttribute?.NamedArguments
                    .FirstOrDefault(x => x.Key == GeneratorConstants.FIELDATTRIBUTE_ARRAY_LENGTH_NAME).Value.Value;
            var isCustomType = IsCustomType(field.Type);
            var isEnum = field.Type.BaseType.GetFullName() == typeof(Enum).FullName;
            var info = new PacketFieldInfo
            {
                Name = field.Name,
                Order = order,
                IsArray = isArray,
                ArrayLength = arrayLength,
                IsCustom = isCustomType,
                TypeFullName = field.Type.GetFullName()!,
                IsEnum = isEnum,
                ElementSize = GetStaticSize(field.Type.GetFullName()!),
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

        return [..list];
    }

    /// <summary>
    /// May return 0 if type is dynamic
    /// </summary>
    private static int GetStaticSize(string typeFullName)
    {
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
            default:
                // TODO
                return 0;
        }
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
