using System.Collections.Immutable;
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

    internal ImmutableArray<FieldData> GetFieldsOfType(TypeDeclarationSyntax type)
    {
        var fields = new List<FieldData>();
        var fieldIndex = 0;
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters
                .Select(x => BuildFieldData(type, x.Type!, x.Identifier, ref fieldIndex, null, null, true, false))
            );
        }

        fields.AddRange(type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x =>
            {
                var fieldAttrArgs = x.AttributeLists
                    .SelectMany(attr => attr.Attributes)
                    .FirstOrDefault(attr =>
                        GetTypeInfo(attr).GetFullName() == GeneratorConstants.FIELDATTRIBUTE_FULLNAME
                    )?.ArgumentList!.Arguments;
                var orderStr = fieldAttrArgs?[0].Expression.ToString();
                var lengthAttrExpression = fieldAttrArgs
                    ?.FirstOrDefault(par => par.NameEquals?.Name.Identifier.Text == "Length")?.Expression;
                int? stringLengthAttr = null;
                if (lengthAttrExpression is BinaryExpressionSyntax or MemberAccessExpressionSyntax)
                {
                    stringLengthAttr = GetConstantValue(type, lengthAttrExpression);
                }
                else if (lengthAttrExpression is LiteralExpressionSyntax literalExpression)
                {
                    if (int.TryParse(literalExpression.GetText().ToString(), out var lengthAttrParsed))
                    {
                        stringLengthAttr = lengthAttrParsed;
                    }
                }

                var arrayLength = GetArrayLength(x);
                return BuildFieldData(type, x.Type, x.Identifier, ref fieldIndex, arrayLength, orderStr, false,
                    x is {ExpressionBody: not null}, stringLengthAttr);
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
                throw new DiagnosticException("QCX000004",
                    $"Field cannot have a higher number ({desPosition}) than actual fields count {fields.Count}",
                    field.SyntaxNode.GetLocation());
            }

            try
            {
                finalArr.Insert(desPosition, field);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new DiagnosticException("QCX000005",
                    $"Field configuration for type {type.Identifier.Text} is invalid", field.SyntaxNode.GetLocation());
            }
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
                    throw new DiagnosticException("QCX000003",
                        "Size fields must be have a position before their array", field.SyntaxNode.GetLocation());
                }
            }
        }

        return finalArr.OrderBy(x => x.Order).ToImmutableArray();
    }

    private int? GetConstantValue(TypeDeclarationSyntax type, ExpressionSyntax expression)
    {
        if (expression is BinaryExpressionSyntax binaryExpressionSyntax)
        {
            // simple a + b recursion
            return new[]
                {
                    binaryExpressionSyntax.Left,
                    binaryExpressionSyntax.Right
                }
                .Select(exp => GetConstantValue(type, exp))
                .Sum();
        }

        var value = Type.SemanticModel.Compilation
            .GetSemanticModel(type.SyntaxTree)
            .GetConstantValue(expression);
        int? stringLengthAttr;
        if (value is {HasValue: true})
        {
            stringLengthAttr = value.Value switch
            {
                byte byteValue => byteValue,
                ushort ushortValue => ushortValue,
                short shortValue => shortValue,
                int intValue => intValue,
                uint uintValue => (int?) uintValue,
                long longValue => (int) longValue,
                ulong longValue => (int) longValue,
                _ => null
            };
        }
        else
        {
            throw new NotImplementedException(
                $"Cannot get value for constant with value {value.Value}. Only integers are supported.");
        }

        return stringLengthAttr;
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
            return (int?) arrayCreationExpressionSyntax.Type.RankSpecifiers
                .First().Sizes.OfType<LiteralExpressionSyntax>()
                .First().Token.Value;
        }

        return null;
    }

    private FieldData BuildFieldData(TypeDeclarationSyntax declaringType, TypeSyntax type, SyntaxToken name,
        ref int fieldIndex,
        int? arrayLength = null,
        string? orderStr = null, bool isRecordParameter = false, bool isReadonly = false, int? stringLength = null)
    {
        var isArray = type is ArrayTypeSyntax;
        var fieldType = GetTypeInfo(type)!;
        var enumType = isArray ? null : (INamedTypeSymbol) fieldType;
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
            throw new DiagnosticException("QCX000002",
                "String or array must have a defined a static length either via FieldAttribute or an array constructor as default value. Dynamic fields must have a field that refers to it's size like \"public uint Size => Message.Length;\"",
                name.Parent!.GetLocation());
        }

        var order = int.TryParse(orderStr, out var orderVal) && orderVal != fieldIndex
            ? orderVal
            : fieldIndex++;

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
            Order = order,
            SizeFieldName = sizeFieldName
        };
    }

    private int GetStaticSize(ITypeSymbol semanticType, int? arrayLength = null)
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
            RecordDeclarationSyntax {ClassOrStructKeyword.Text: "struct"} => "record struct",
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
                           GetTypeInfo(dt.Parent!).GetFullName() == "QuantumCore.Networking.PacketAttribute"));

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

    public (byte Value, int Position)? GetSubHeaderForType(TypeDeclarationSyntax type)
    {
        var attr = type
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a => a
                .DescendantTokens()
                .Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) &&
                           GetTypeInfo(dt.Parent!).GetFullName() == GeneratorConstants.SUBPACKETATTRIBUTE_FULLNAME));
        var value = attr?.ArgumentList?.Arguments[0].ToString();
        if (value is null) return null;
        var position = attr?.ArgumentList?.Arguments.ElementAtOrDefault(1)?.ToString();
        return (Convert.ToByte(value, 16), position is not null ? Convert.ToInt32(position) : 0);
    }

    public bool HasTypeSequence(TypeDeclarationSyntax type)
    {
        var attr = type.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(x => x.Name.ToString() == "Packet");
        return attr?.ArgumentList?.Arguments.Any(x =>
            x.NameEquals?.Name.Identifier.Text == "Sequence" &&
            ((LiteralExpressionSyntax) x.Expression).Token.Text == "true") ?? false;
    }
}
