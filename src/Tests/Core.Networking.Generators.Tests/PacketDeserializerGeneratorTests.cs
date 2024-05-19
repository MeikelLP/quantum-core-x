using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class PacketDeserializerGeneratorTests
{
    [Theory]
    [InlineData(4, null, false,
        "(bytes[(System.Index)(offset + 0)..(System.Index)(offset + 0 + 4)]).ReadNullTerminatedString()")]
    [InlineData(4, null, true, "await stream.ReadStringFromStreamAsync(buffer, (int)4)")]
    [InlineData(0, "TestificateSize", false,
        "(bytes[(System.Index)(offset + 0)..(System.Index)(offset + 0 + __TestificateSize)]).ReadNullTerminatedString()")]
    public void GetValueForString(int elementSize, string? sizeFieldName, bool isStreamMode, string expected)
    {
        var offset = 0;
        var dynamicOffset = new StringBuilder();
        var namedTypeSymbol = Substitute.For<INamedTypeSymbol>();
        namedTypeSymbol.Name.Returns("String");
        var result = DeserializeGenerator.GetValueForString(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = elementSize,
            Order = null,
            SemanticType = namedTypeSymbol,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, "", isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, "", "", null, false, "(System.Index)(offset + 0)")]
    [InlineData(2, "", "", null, false, "(System.Index)(offset + 2)")]
    [InlineData(2, " + abc", "", null, false, "(System.Index)(offset + 2 + abc)")]
    [InlineData(2, " + abc", " + def", null, false, "(System.Index)(offset + 2 + abc + def)")]
    [InlineData(2, " + abc", " + def", 4, false,
        "(System.Index)(offset + 2 + abc + def)..(System.Index)(offset + 2 + abc + def + 4)")]
    [InlineData(2, "", "", 4, false, "(System.Index)(offset + 2)..(System.Index)(offset + 2 + 4)")]
    [InlineData(2, "", "", 4, true, "(System.Index)(2)..(System.Index)(2 + 4)")]
    [InlineData(2, " + abc", " + def", 4, true, "(System.Index)(2 + abc + def)..(System.Index)(2 + abc + def + 4)")]
    public void GetOffsetString(int offset, string dynamicOffsetStart, string tempDynamicOffset, int? arrayLength,
        bool doNotPrependOffsetVariable, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetOffsetString(offset, dynamicOffset, tempDynamicOffset, arrayLength,
            doNotPrependOffsetVariable);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, 2, "", "", false, "bytes[(System.Index)(offset + 0)..(System.Index)(offset + 2)].ToArray()")]
    [InlineData(2, 2, "", "", false, "bytes[(System.Index)(offset + 2)..(System.Index)(offset + 4)].ToArray()")]
    [InlineData(2, 2, " + abc", "", false,
        "bytes[(System.Index)(offset + 2 + abc)..(System.Index)(offset + 4 + abc)].ToArray()")]
    [InlineData(2, 2, " + abc", " + def", false,
        "bytes[(System.Index)(offset + 2 + abc + def)..(System.Index)(offset + 4 + abc + def)].ToArray()")]
    [InlineData(2, 4, "", "", false, "bytes[(System.Index)(offset + 2)..(System.Index)(offset + 6)].ToArray()")]
    [InlineData(2, 2, "", "", true, "await stream.ReadByteArrayFromStreamAsync(buffer, 2)")]
    [InlineData(2, 4, "", "", true, "await stream.ReadByteArrayFromStreamAsync(buffer, 4)")]
    public void GetLineForFixedByteArray(int offset, int arrayLength, string dynamicOffsetStart,
        string tempDynamicOffset, bool isStreamMode, string expected)
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetLineForFixedByteArray(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = arrayLength,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, "TestificateLength", "", "", false,
        "bytes[(System.Index)(offset + 0)..(System.Index)(offset + 0 + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", "", "", false,
        "bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", " + abc", "", false,
        "bytes[(System.Index)(offset + 2 + abc)..(System.Index)(offset + 2 + abc + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", " + abc", " + def", false,
        "bytes[(System.Index)(offset + 2 + abc + def)..(System.Index)(offset + 2 + abc + __TestificateLength + def)].ToArray()")]
    [InlineData(2, "TestificateLength", "", "", true,
        "await stream.ReadByteArrayFromStreamAsync(buffer, (int)__TestificateLength)")]
    public void GetLineForDynamicByteArray(int offset, string sizeFieldName, string dynamicOffsetStart,
        string tempDynamicOffset, bool isStreamMode, string expected)
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetLineForDynamicByteArray(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("String", "TestificateLength",
        "(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + __TestificateLength)]).ReadNullTerminatedString()")]
    [InlineData("Byte", null, "bytes[(System.Index)(offset + 2)]")]
    [InlineData("SByte", null, "bytes[(System.Index)(offset + 2)]")]
    [InlineData("Boolean", null, "System.Convert.ToBoolean(bytes[(System.Index)(offset + 2)])")]
    [InlineData("Int16", null,
        "System.BitConverter.ToInt16(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 2)])")]
    [InlineData("Int32", null,
        "System.BitConverter.ToInt32(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 4)])")]
    [InlineData("Int64", null,
        "System.BitConverter.ToInt64(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 8)])")]
    public void GetValueForSingleValue(string type, string? sizeFieldName, string expected)
    {
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.ContainingNamespace.Name.Returns("System");
        semanticType.Name.Returns(type);
        var dynamicOffset = new StringBuilder();
        var result = DeserializeGenerator.GetValueForSingleValue(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, ref offset, dynamicOffset, "", false);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("Byte", "", "(QuantumCore.EPhases)bytes[(System.Index)(offset + 2)]")]
    [InlineData("SByte", "", "(QuantumCore.EPhases)bytes[(System.Index)(offset + 2)]")]
    [InlineData("Boolean", "", "(QuantumCore.EPhases)System.Convert.ToBoolean(bytes[(System.Index)(offset + 2)])")]
    [InlineData("Int16", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt16(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 2)])")]
    [InlineData("Int32", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt32(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 4)])")]
    [InlineData("Int64", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt64(bytes[(System.Index)(offset + 2)..(System.Index)(offset + 2 + 8)])")]
    public void GetValueForSingleValue_Enum(string underlyingType, string sizeFieldName, string expected)
    {
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("EPhases");
        semanticType.TypeKind.Returns(TypeKind.Enum);
        semanticType.GetFullNamespace().Returns("QuantumCore");
        semanticType.EnumUnderlyingType!.Name.Returns(underlyingType);
        var dynamicOffset = new StringBuilder();
        var result = DeserializeGenerator.GetValueForSingleValue(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = true,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, ref offset, dynamicOffset, "", false);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("TestificateLength", "__TestificateLength")]
    [InlineData("Testificate.Length", "__Testificate_Length")]
    public void GetVariableNameForExpression(string input, string expected)
    {
        var result = DeserializeGenerator.GetVariableNameForExpression(input);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("String", 0, "TestificateLength",
        "await stream.ReadStringFromStreamAsync(buffer, (int)__TestificateLength)")]
    [InlineData("String", 4, null, "await stream.ReadStringFromStreamAsync(buffer, (int)4)")]
    [InlineData("Byte", 0, null, "await stream.ReadValueFromStreamAsync<Byte>(buffer)")]
    [InlineData("Half", 0, null, "await stream.ReadValueFromStreamAsync<Half>(buffer)")]
    [InlineData("Single", 0, null, "await stream.ReadValueFromStreamAsync<Single>(buffer)")]
    [InlineData("Double", 0, null, "await stream.ReadValueFromStreamAsync<Double>(buffer)")]
    [InlineData("Int16", 0, null, "await stream.ReadValueFromStreamAsync<Int16>(buffer)")]
    [InlineData("Int32", 0, null, "await stream.ReadValueFromStreamAsync<Int32>(buffer)")]
    [InlineData("Int64", 0, null, "await stream.ReadValueFromStreamAsync<Int64>(buffer)")]
    [InlineData("UInt16", 0, null, "await stream.ReadValueFromStreamAsync<UInt16>(buffer)")]
    [InlineData("UInt32", 0, null, "await stream.ReadValueFromStreamAsync<UInt32>(buffer)")]
    [InlineData("UInt64", 0, null, "await stream.ReadValueFromStreamAsync<UInt64>(buffer)")]
    public void GetStreamReaderLine(string fieldTypeName, int elementSize, string sizeFieldName, string expected)
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns(fieldTypeName);
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = elementSize,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("Byte", null, "TestificateLength",
        "await stream.ReadByteArrayFromStreamAsync(buffer, __TestificateLength)")]
    [InlineData("Byte", 4, null, "await stream.ReadByteArrayFromStreamAsync(buffer, 4)")]
    [InlineData("SByte", null, null, "await stream.ReadValueFromStreamAsync<SByte>(buffer)")]
    [InlineData("Int16", null, null, "await stream.ReadValueFromStreamAsync<Int16>(buffer)")]
    [InlineData("Int32", null, null, "await stream.ReadValueFromStreamAsync<Int32>(buffer)")]
    [InlineData("Int64", null, null, "await stream.ReadValueFromStreamAsync<Int64>(buffer)")]
    [InlineData("Half", null, null, "await stream.ReadValueFromStreamAsync<Half>(buffer)")]
    [InlineData("Single", null, null, "await stream.ReadValueFromStreamAsync<Single>(buffer)")]
    [InlineData("Double", null, null, "await stream.ReadValueFromStreamAsync<Double>(buffer)")]
    public void GetStreamReaderLine_Array(string arrayType, int? arrayLength, string sizeFieldName, string expected)
    {
        var semanticType = Substitute.For<IArrayTypeSymbol>();
        semanticType.ElementType.Name.Returns(arrayType);
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            Name = "",
            IsArray = true,
            IsEnum = false,
            ArrayLength = arrayLength,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(1, "(Test.CustomEnum) await stream.ReadEnumFromStreamAsync<Test.CustomEnum>(buffer)")]
    public void GetStreamReaderLine_Enum(int elementSize, string expected)
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("CustomEnum");
        semanticType.GetFullNamespace().Returns("Test");
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = true,
            ArrayLength = 0,
            ElementSize = elementSize,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetStreamReaderLine_ThrowsIf_UnknownType()
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("Testificate");
        var ex = Assert.Throws<ArgumentException>(() => DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            Name = "Test",
            IsArray = false,
            IsEnum = false,
            ArrayLength = 0,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false
        }));

        ex.Message.Should().BeEquivalentTo("Don't know how to handle type of field Testificate");
    }

    [Fact]
    public void GetStreamReaderLine_ThrowsIf_ArrayOfUnknownType()
    {
        var semanticType = Substitute.For<IArrayTypeSymbol>();
        semanticType.ElementType.Name.Returns("CustomType");
        var ex = Assert.Throws<ArgumentException>(() => DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            Name = "Testificate",
            IsArray = true,
            IsEnum = false,
            ArrayLength = 0,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false
        }));

        ex.Message.Should().BeEquivalentTo("Don't know how to handle type of field CustomType");
    }

    [Theory]
    [InlineData("Byte", 1)]
    [InlineData("SByte", 1)]
    [InlineData("Boolean", 1)]
    [InlineData("Int16", 2)]
    [InlineData("Int32", 4)]
    [InlineData("Int64", 8)]
    [InlineData("Half", 2)]
    [InlineData("Single", 4)]
    [InlineData("Double", 8)]
    public static void GetSizeOfPrimitiveType(string type, int expected)
    {
        GeneratorConstants.GetSizeOfPrimitiveType(type).Should().Be(expected);
    }

    [Fact]
    public static void GetSizeOfPrimitiveType_ThrowsIfUnknown()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => GeneratorConstants.GetSizeOfPrimitiveType("Unknown"));

        ex.Message.Should().BeEquivalentTo("Don't know the size of Unknown (Parameter 'name')");
    }
}
