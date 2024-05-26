using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class ClientToServerPacketGeneratorTests
{
    [Theory]
    [InlineData(4, null, false,
        "bytes[0..(0 + 4)].ReadNullTerminatedString()")]
    [InlineData(4, null, true, "await stream.ReadStringFromStreamAsync(buffer, (int)4)")]
    [InlineData(0, "TestificateSize", false,
        "bytes[0..(0 + __TestificateSize)].ReadNullTerminatedString()")]
    public void GetValueForString(int elementSize, string? sizeFieldName, bool isStreamMode, string expected)
    {
        var offset = 0;
        var dynamicOffset = new StringBuilder();
        var result = DeserializeGenerator.GetValueForString(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = elementSize,
            TypeFullName = "System.String",
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, "", isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, "", "", null, "0")]
    [InlineData(2, "", "", null, "2")]
    [InlineData(2, " + abc", "", null, "(2 + abc)")]
    [InlineData(2, " + abc", " + def", null, "(2 + abc + def)")]
    [InlineData(2, " + abc", " + def", 4,
        "(2 + abc + def)..(2 + abc + def + 4)")]
    [InlineData(2, "", "", 4, "2..(2 + 4)")]
    [InlineData(2, " + abc", " + def", 4, "(2 + abc + def)..(2 + abc + def + 4)")]
    public void GetOffsetString(int offset, string dynamicOffsetStart, string tempDynamicOffset, int? arrayLength,
        string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetOffsetString(offset, dynamicOffset, tempDynamicOffset, arrayLength);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, 2, "", "", false, "bytes[0..2].ToArray()")]
    [InlineData(2, 2, "", "", false, "bytes[2..4].ToArray()")]
    [InlineData(2, 2, " + abc", "", false,
        "bytes[(2 + abc)..(4 + abc)].ToArray()")]
    [InlineData(2, 2, " + abc", " + def", false,
        "bytes[(2 + abc + def)..(4 + abc + def)].ToArray()")]
    [InlineData(2, 4, "", "", false, "bytes[2..6].ToArray()")]
    [InlineData(2, 2, "", "", true, "await stream.ReadByteArrayFromStreamAsync(buffer, 2)")]
    [InlineData(2, 4, "", "", true, "await stream.ReadByteArrayFromStreamAsync(buffer, 4)")]
    public void GetLineForFixedByteArray(int offset, int arrayLength, string dynamicOffsetStart,
        string tempDynamicOffset, bool isStreamMode, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetLineForFixedByteArray(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = arrayLength,
            ElementSize = 0,
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0, "TestificateLength", "", "", false,
        "bytes[0..(0 + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", "", "", false,
        "bytes[2..(2 + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", " + abc", "", false,
        "bytes[(2 + abc)..(2 + abc + __TestificateLength)].ToArray()")]
    [InlineData(2, "TestificateLength", " + abc", " + def", false,
        "bytes[(2 + abc + def)..(2 + abc + __TestificateLength + def)].ToArray()")]
    [InlineData(2, "TestificateLength", "", "", true,
        "await stream.ReadByteArrayFromStreamAsync(buffer, (int)__TestificateLength)")]
    public void GetLineForDynamicByteArray(int offset, string sizeFieldName, string dynamicOffsetStart,
        string tempDynamicOffset, bool isStreamMode, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = DeserializeGenerator.GetLineForDynamicByteArray(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("System.String", "TestificateLength",
        "(bytes[2..(2 + __TestificateLength)]).ReadNullTerminatedString()")]
    [InlineData("System.Byte", null, "bytes[2]")]
    [InlineData("System.SByte", null, "bytes[2]")]
    [InlineData("System.Boolean", null, "System.Convert.ToBoolean(bytes[2])")]
    [InlineData("System.Int16", null,
        "System.BitConverter.ToInt16(bytes[2..(2 + 2)])")]
    [InlineData("System.Int32", null,
        "System.BitConverter.ToInt32(bytes[2..(2 + 4)])")]
    [InlineData("System.Int64", null,
        "System.BitConverter.ToInt64(bytes[2..(2 + 8)])")]
    public void GetValueForSingleValue(string type, string? sizeFieldName, string expected)
    {
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.ContainingNamespace.Name.Returns("System");
        semanticType.Name.Returns(type);
        var dynamicOffset = new StringBuilder();
        var result = DeserializeGenerator.GetValueForSingleValue(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = type,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, "", false);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("Byte", "", "(QuantumCore.EPhases)bytes[2]")]
    [InlineData("SByte", "", "(QuantumCore.EPhases)bytes[2]")]
    [InlineData("Boolean", "", "(QuantumCore.EPhases)System.Convert.ToBoolean(bytes[2])")]
    [InlineData("Int16", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt16(bytes[2..(2 + 2)])")]
    [InlineData("Int32", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt32(bytes[2..(2 + 4)])")]
    [InlineData("Int64", "",
        "(QuantumCore.EPhases)System.BitConverter.ToInt64(bytes[2..(2 + 8)])")]
    public void GetValueForSingleValue_Enum(string underlyingType, string sizeFieldName, string expected)
    {
        var offset = 2;
        var dynamicOffset = new StringBuilder();
        var result = DeserializeGenerator.GetValueForSingleValue(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = true,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = "QuantumCore.EPhases",
            ElementTypeFullName = $"System.{underlyingType}",
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false,
        }, ref offset, dynamicOffset, "", false);

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
    [InlineData("System.String", 0, "TestificateLength",
        "await stream.ReadStringFromStreamAsync(buffer, (int)__TestificateLength)")]
    [InlineData("System.String", 4, null, "await stream.ReadStringFromStreamAsync(buffer, (int)4)")]
    [InlineData("System.Byte", 0, null, "await stream.ReadValueFromStreamAsync<System.Byte>(buffer)")]
    [InlineData("System.Half", 0, null, "await stream.ReadValueFromStreamAsync<System.Half>(buffer)")]
    [InlineData("System.Single", 0, null, "await stream.ReadValueFromStreamAsync<System.Single>(buffer)")]
    [InlineData("System.Double", 0, null, "await stream.ReadValueFromStreamAsync<System.Double>(buffer)")]
    [InlineData("System.Int16", 0, null, "await stream.ReadValueFromStreamAsync<System.Int16>(buffer)")]
    [InlineData("System.Int32", 0, null, "await stream.ReadValueFromStreamAsync<System.Int32>(buffer)")]
    [InlineData("System.Int64", 0, null, "await stream.ReadValueFromStreamAsync<System.Int64>(buffer)")]
    [InlineData("System.UInt16", 0, null, "await stream.ReadValueFromStreamAsync<System.UInt16>(buffer)")]
    [InlineData("System.UInt32", 0, null, "await stream.ReadValueFromStreamAsync<System.UInt32>(buffer)")]
    [InlineData("System.UInt64", 0, null, "await stream.ReadValueFromStreamAsync<System.UInt64>(buffer)")]
    public void GetStreamReaderLine(string fieldTypeName, int elementSize, string sizeFieldName, string expected)
    {
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns(fieldTypeName);
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = elementSize,
            TypeFullName = fieldTypeName,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("System.Byte", null, "TestificateLength",
        "await stream.ReadByteArrayFromStreamAsync(buffer, __TestificateLength)")]
    [InlineData("System.Byte", 4, null, "await stream.ReadByteArrayFromStreamAsync(buffer, 4)")]
    [InlineData("System.SByte", null, null, "await stream.ReadValueFromStreamAsync<System.SByte>(buffer)")]
    [InlineData("System.Int16", null, null, "await stream.ReadValueFromStreamAsync<System.Int16>(buffer)")]
    [InlineData("System.Int32", null, null, "await stream.ReadValueFromStreamAsync<System.Int32>(buffer)")]
    [InlineData("System.Int64", null, null, "await stream.ReadValueFromStreamAsync<System.Int64>(buffer)")]
    [InlineData("System.Half", null, null, "await stream.ReadValueFromStreamAsync<System.Half>(buffer)")]
    [InlineData("System.Single", null, null, "await stream.ReadValueFromStreamAsync<System.Single>(buffer)")]
    [InlineData("System.Double", null, null, "await stream.ReadValueFromStreamAsync<System.Double>(buffer)")]
    public void GetStreamReaderLine_Array(string arrayType, int? arrayLength, string sizeFieldName, string expected)
    {
        var semanticType = Substitute.For<IArrayTypeSymbol>();
        semanticType.ElementType.Name.Returns(arrayType);
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            FieldName = "",
            IsArray = true,
            IsEnum = false,
            ArrayLength = arrayLength,
            ElementSize = 0,
            ElementTypeFullName = arrayType,
            SizeFieldName = sizeFieldName,
            IsCustom = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(1, "(Test.CustomEnum) await stream.ReadEnumFromStreamAsync<Test.CustomEnum>(buffer)")]
    public void GetStreamReaderLine_Enum(int elementSize, string expected)
    {
        var result = DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = true,
            ArrayLength = 0,
            ElementSize = elementSize,
            TypeFullName = "Test.CustomEnum",
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false
        });

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetStreamReaderLine_ThrowsIf_UnknownType()
    {
        var ex = Assert.Throws<ArgumentException>(() => DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            FieldName = "Test",
            IsArray = false,
            IsEnum = false,
            ArrayLength = 0,
            ElementSize = 0,
            TypeFullName = "Testificate",
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false
        }));

        ex.Message.Should().BeEquivalentTo("Don't know how to handle type of field Testificate");
    }

    [Fact]
    public void GetStreamReaderLine_ThrowsIf_ArrayOfUnknownType()
    {
        var ex = Assert.Throws<ArgumentException>(() => DeserializeGenerator.GetStreamReaderLine(new FieldData
        {
            FieldName = "Testificate",
            IsArray = true,
            IsEnum = false,
            ArrayLength = 0,
            ElementSize = 0,
            ElementTypeFullName = "CustomType",
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false
        }));

        ex.Message.Should().BeEquivalentTo("Don't know how to handle type of field CustomType");
    }

    [Theory]
    [InlineData("System.Byte", 1)]
    [InlineData("System.SByte", 1)]
    [InlineData("System.Boolean", 1)]
    [InlineData("System.Int16", 2)]
    [InlineData("System.Int32", 4)]
    [InlineData("System.Int64", 8)]
    [InlineData("System.Half", 2)]
    [InlineData("System.Single", 4)]
    [InlineData("System.Double", 8)]
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