using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal class GeneratorContext
{
    public SerializerTypeInfo Type { get; }

    internal GeneratorContext(SerializerTypeInfo type)
    {
        Type = type;
    }

    // private IReadOnlyDictionary<string, (TypeDeclarationSyntax TypeDeclaration, bool GenerateFor)> GetRelevantTypes(
    //     IEnumerable<SyntaxTree> syntaxTrees)
    // {
    //     var allTypeDeclarations = syntaxTrees
    //         .SelectMany(x => x.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
    //         .ToArray();
    //     Dictionary<string, (TypeDeclarationSyntax Type, bool ShouldGenerateFor)> typesToGenerateFor;
    //     typesToGenerateFor = allTypeDeclarations
    //         .Where(x => x
    //             .AttributeLists
    //             .SelectMany(list => list.Attributes)
    //             .Any(attr => GetTypeInfo(attr).GetFullName(), GeneratorAttributeType)
    //         )
    //         .GroupBy(x => GetTypeInfo(x).GetFullName()!)
    //         .ToDictionary(x => x.Key, x => (Type: x.First(), ShouldGenerateFor: true));
    //     var noGeneratorButRelevantTypes = new Dictionary<string, (TypeDeclarationSyntax, bool)>();
    //     foreach (var keyPair in typesToGenerateFor)
    //     {
    //         var fields = GetMemberDefinitions(keyPair.Value.Type);
    //         var includedCustomTypes = fields
    //             .Select(x =>
    //             {
    //                 var typeInfo = GetTypeInfo(x);
    //                 if (typeInfo is IArrayTypeSymbol arr && IsCustomType(arr.ElementType))
    //                 {
    //                     return arr.ElementType;
    //                 }
    //
    //                 return typeInfo;
    //             })
    //             .Where(IsCustomType!)
    //             .GroupBy(x => x.GetFullName())
    //             .Select(x => x.First())
    //             .ToArray();
    //         foreach (var includedCustomType in includedCustomTypes)
    //         {
    //             var includedCustomTypeSymbol = allTypeDeclarations.FirstOrDefault(x =>
    //                                  SymbolEqualityComparer.Default.Equals(GetTypeInfo(x), includedCustomType))
    //                              ?? throw new InvalidOperationException(
    //                                  "Type cannot be used as it is not defined in the same assembly as packet type");
    //             var fullName = GetTypeInfo(includedCustomTypeSymbol)!.GetFullName()!;
    //             if (!noGeneratorButRelevantTypes.ContainsKey(fullName))
    //             {
    //                 noGeneratorButRelevantTypes.Add(fullName, (includedCustomTypeSymbol, false));
    //             }
    //         }
    //     }
    //
    //     return typesToGenerateFor!
    //         .Concat(noGeneratorButRelevantTypes)
    //         .OrderBy(x => x.Key)
    //         .ToDictionary(x => x.Key, x => x.Value);
    // }

    private IEnumerable<TypeSyntax> GetMemberDefinitions(TypeDeclarationSyntax type)
    {
        var fields = new List<TypeSyntax>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters.Select(p => p.Type!));
        }

        fields.AddRange(type
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(x => x.Type));
        fields.AddRange(type
            .DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Select(x => x.Declaration.Type));

        return fields;
    }
    
    

    internal IReadOnlyList<FieldData> GetFieldsOfType(TypeDeclarationSyntax type)
    {
        var fields = new List<FieldData>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters
                .Select(x => BuildFieldData(type, x.Type!, x.Identifier, null, null, true, false))
            );
        }

        fields.AddRange(type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x =>
            {
                var fieldAttrArgs = x.AttributeLists
                    .SelectMany(attr => attr.Attributes)
                    .FirstOrDefault(attr =>
                        GetTypeInfo(attr).GetFullName() == "QuantumCore.Core.Networking.FieldAttribute"
                    )?.ArgumentList!.Arguments;
                var orderStr = fieldAttrArgs?[0].Expression.ToString();
                var lengthAttrStr = fieldAttrArgs?.FirstOrDefault(par => par.NameEquals?.Name.Identifier.Text == "Length")?.Expression.ToString();
                int? stringLengthAttr = null;
                if (int.TryParse(lengthAttrStr, out var lengthAttrParsed))
                {
                    stringLengthAttr = lengthAttrParsed;
                }
                var arrayLength = GetArrayLength(x);
                return BuildFieldData(type, x.Type, x.Identifier, arrayLength, orderStr, false, x is { ExpressionBody: not null }, stringLengthAttr);
            })
        );
        
        var finalArr = new List<FieldData>(fields.Count);
        // first add all fields normally
        finalArr.AddRange(fields.Where(x => !x.Order.HasValue));
        // then insert overriden fields to their desired position
        foreach (var field in fields.Where(x => x.Order.HasValue).OrderBy(x => x.Order))
        {
            var desPosition = field.Order!.Value;
            if (desPosition >= fields.Count)
            {
                throw new InvalidOperationException(
                    $"Field cannot have a higher number ({desPosition}) than actual fields count {fields.Count}");
            }
            finalArr.Insert(desPosition, field);
        }

        for (var i = 0; i < finalArr.Count; i++)
        {
            var field = finalArr[i];
            var sizeFieldName = field.SizeFieldName;
            if (sizeFieldName is not null)
            {
                var index = finalArr.FindIndex(x => x.Name == sizeFieldName);
                if (index > i)
                {
                    throw new DiagnosticException("QCX-G000003", "Size fields must be have a position before their array", field.SyntaxNode.GetLocation());
                }
            }
        }

        return finalArr;
    }

    private static int? GetArrayLength(PropertyDeclarationSyntax x)
    {
        if (x is
            {
                Type: ArrayTypeSyntax, Initializer: not null, Initializer.Value: ArrayCreationExpressionSyntax
                {
                    Type.RankSpecifiers.Count: 1
                } arrayCreationExpressionSyntax
            } && 
            arrayCreationExpressionSyntax.Type.RankSpecifiers.Any() && 
            arrayCreationExpressionSyntax.Type.RankSpecifiers.First().Sizes.OfType<LiteralExpressionSyntax>().Any())
        {
            return (int?)arrayCreationExpressionSyntax.Type.RankSpecifiers
                .First().Sizes.OfType<LiteralExpressionSyntax>()
                .First().Token.Value;
        }

        return null;
    }

    private FieldData BuildFieldData(TypeDeclarationSyntax declaringType, TypeSyntax type, SyntaxToken name, int? arrayLength = null,
        string? orderStr = null, bool isRecordParameter = false, bool isReadonly = false, int? stringLength = null)
    {
        var isArray = type is ArrayTypeSyntax;
        var fieldType = GetTypeInfo(type)!;
        var enumType = isArray ? null : (INamedTypeSymbol)fieldType;
        var isEnum = enumType?.TypeKind is TypeKind.Enum;
        string? sizeFieldName = null;
        
        if (fieldType.Name == "String" || isArray)
        {
            var expression = declaringType.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(x => x.ExpressionBody?.Expression
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Any(n => n.Identifier.Text == name.Text) ?? false);
            if (expression is not null)
            {
                sizeFieldName = expression.Identifier.Text;
            }
        }

        var elementSize = GetStaticSize(fieldType, arrayLength);
        if ((type is ArrayTypeSyntax || fieldType.Name == "String") &&
            stringLength is not null)
        {
            elementSize = stringLength.Value;
        }
        if ((fieldType.Name == "String" && stringLength is null && sizeFieldName is null) ||
            (type is ArrayTypeSyntax && sizeFieldName is null && arrayLength is null))
        {
            throw new DiagnosticException("QCX-G000002",
                "String or array must have a defined a static length either via FieldAttribute or an array constructor as default value. Dynamic fields must have a field that refers to it's size like \"public uint Size => Message.Length;\"",
                name.Parent!.GetLocation());
        } 

        return new FieldData
        {
            Name = name.Text,
            SemanticType = fieldType,
            SyntaxNode = name.Parent!,
            IsArray = isArray,
            IsEnum = isEnum,
            IsCustom = IsCustomType(fieldType),
            ArrayLength = arrayLength,
            ElementSize = elementSize,
            IsRecordParameter = isRecordParameter,
            IsReadonly = isReadonly,
            Order = orderStr != null ? int.Parse(orderStr) : null,
            SizeFieldName = sizeFieldName
        };
    }
    
    private int GetStaticSize(ITypeSymbol semanticType, int? arrayLength = null)
    {
        var typeName = semanticType.Name;

        switch (typeName)
        {
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
                    var customType = GetTypeDeclaration(semanticType);
                    var fields = GetFieldsOfType(customType);
                    return GetStaticSizeOfType(fields);
                }
                else if (semanticType is IArrayTypeSymbol arr)
                {
                    if (arrayLength is null)
                    {
                        // may have dynamic size
                        return GetStaticSize(arr.ElementType);
                    }
                    else
                    {
                        return GetStaticSize(arr.ElementType);
                    }
                }
                else if (semanticType.TypeKind is TypeKind.Enum && semanticType is INamedTypeSymbol namedTypeSymbol)
                {
                    return GetStaticSize(namedTypeSymbol.EnumUnderlyingType!);
                }

                throw new NotImplementedException($"Don't know how to handle {semanticType.Name}");
        }
    }

    public TypeDeclarationSyntax GetTypeDeclaration(ITypeSymbol semanticType)
    {
        return semanticType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .First();
    }

    internal ITypeSymbol? GetTypeInfo(BaseTypeDeclarationSyntax type)
    {
        if (Type.SemanticModel.SyntaxTree == type.SyntaxTree)
        {
            return Type.SemanticModel.GetDeclaredSymbol(type);
        }
        else
        {
            return Type.SemanticModel.Compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type);
        }
    }

    internal ITypeSymbol? GetTypeInfo(SyntaxNode type)
    {
        if (Type.SemanticModel.SyntaxTree == type.SyntaxTree)
        {
            return Type.SemanticModel.GetTypeInfo(type).Type;
        }
        else
        {
            return Type.SemanticModel.Compilation.GetSemanticModel(type.SyntaxTree).GetTypeInfo(type).Type;
        }
    }

    internal static int GetStaticSizeOfType(IReadOnlyList<FieldData> fields)
    {
        return fields.Sum(x => x.FieldSize);
    }

    internal static string GetTypeKeyWords(TypeDeclarationSyntax type)
    {
        var typeKeyWords = type switch
        {
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax { ClassOrStructKeyword.Text: "struct" } => "record struct",
            RecordDeclarationSyntax => "record",
            _ => "class"
        };

        return typeKeyWords;
    }

    internal static bool IsCustomType(ITypeSymbol fieldType)
    {
        return !fieldType.GetFullName()!.StartsWith("System.") &&
               fieldType.TypeKind is not TypeKind.Enum and not TypeKind.Array;
    }

    public string GetHeaderForType(TypeDeclarationSyntax type)
    {
        var attr = type
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a => a
                .DescendantTokens()
                .Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) &&
                           GetTypeInfo(dt.Parent!).GetFullName() == "QuantumCore.Core.Networking.PacketAttribute"));

        if (attr is null)
        {
            throw new InvalidOperationException(
                "PacketGeneratorAttribute requires PacketAttribute to be set as well");
        }
        
        if (attr.ArgumentList!.Arguments.Count == 0)
        {
            throw new InvalidOperationException("PacketGeneratorAttribute must have parameters defined");
        }
        return attr.ArgumentList.Arguments[0].ToString();
    }
}