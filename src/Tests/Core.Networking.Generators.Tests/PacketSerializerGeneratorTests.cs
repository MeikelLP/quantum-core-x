using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NSubstitute;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class PacketSerializerGeneratorTests
{
    [Theory]
    [InlineData("", 0, "", "", "this.CopyTo(bytes, offset + 0);")]
    [InlineData("", 2, "", "", "this.CopyTo(bytes, offset + 2);")]
    [InlineData("", 2, " + abc", "", "this.CopyTo(bytes, offset + 2 + abc);")]
    [InlineData("", 2, " + abc", " + def", "this.CopyTo(bytes, offset + 2 + abc + def);")]
    [InlineData("  ", 2, " + abc", " + def", "  this.CopyTo(bytes, offset + 2 + abc + def);")]
    public void GetLineForFixedByteArray(string indentPrefix, int offset, string dynamicOffsetStart, string tempDynamicOffset, string expected)
    {
        var initialOffset = offset;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForFixedByteArray(new FieldData
        {
            Name = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = 2,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, "this", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset + 2);
    }

    [Theory]
    [InlineData("Byte", "", 0, "", "", "bytes[offset + 0] = this.Testificate;")]
    [InlineData("Byte", "", 2, "", "", "bytes[offset + 2] = this.Testificate;")]
    [InlineData("Byte", "", 2, " + abc", "", "bytes[offset + 2 + abc] = this.Testificate;")]
    [InlineData("Byte", "", 2, " + abc", " + def", "bytes[offset + 2 + abc + def] = this.Testificate;")]
    [InlineData("Byte", "  ", 2, " + abc", " + def", "  bytes[offset + 2 + abc + def] = this.Testificate;")]
    [InlineData("Int16", "", 2, "", "", "bytes[offset + 2] = (System.Byte)(this.Testificate >> 0);\r\n" +
                                        "bytes[offset + 3] = (System.Byte)(this.Testificate >> 8);")]
    [InlineData("Int32", "", 2, "", "", "bytes[offset + 2] = (System.Byte)(this.Testificate >> 0);\r\n" +
                                        "bytes[offset + 3] = (System.Byte)(this.Testificate >> 8);\r\n" +
                                        "bytes[offset + 4] = (System.Byte)(this.Testificate >> 16);\r\n" +
                                        "bytes[offset + 5] = (System.Byte)(this.Testificate >> 24);")]
    public void GetLineForSingleValue(string type, string indentPrefix, int offset, string dynamicOffsetStart, string tempDynamicOffset, string expected)
    {
        // TODO test enum
        // TODO test array
        var initialOffset = offset;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns(type);
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, "this.Testificate", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset + GeneratorConstants.GetSizeOfPrimitiveType(type));
    }

    [Theory]
    [InlineData("Byte", "bytes[offset + 2] = (System.Byte)this.Testificate;")]
    [InlineData("Int16", "bytes[offset + 2] = (System.Byte)((System.Int16)this.Testificate >> 0);\r\n" +
                         "bytes[offset + 3] = (System.Byte)((System.Int16)this.Testificate >> 8);")]
    [InlineData("Int32", "bytes[offset + 2] = (System.Byte)((System.Int32)this.Testificate >> 0);\r\n" +
                         "bytes[offset + 3] = (System.Byte)((System.Int32)this.Testificate >> 8);\r\n" +
                         "bytes[offset + 4] = (System.Byte)((System.Int32)this.Testificate >> 16);\r\n" +
                         "bytes[offset + 5] = (System.Byte)((System.Int32)this.Testificate >> 24);")]
    public void GetLineForSingleValue_Enum(string underlyingType, string expected)
    {
        var initialOffset = 2;
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("EPhase");
        semanticType.EnumUnderlyingType!.Name.Returns(underlyingType);
        semanticType.EnumUnderlyingType!.ContainingNamespace.Name.Returns("System");
        semanticType.ContainingNamespace.Name.Returns("QuantumCore");
        var dynamicOffset = new StringBuilder();
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = true,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, "this.Testificate", ref offset, dynamicOffset, "", "");

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset + GeneratorConstants.GetSizeOfPrimitiveType(underlyingType));
    }

    [Fact]
    public void GetLineForSingleValue_String()
    {
        var initialOffset = 2;
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("String");
        var dynamicOffset = new StringBuilder();
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = "TestificateLength",
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, "this.Testificate", ref offset, dynamicOffset, "", "");

        result.Should().BeEquivalentTo("bytes.WriteString(this.Testificate, offset + 2, (int)this.TestificateLength);");
        offset.Should().Be(initialOffset);
    }

    [Fact]
    public void GetLineForSingleValue_FixedSizeString()
    {
        var initialOffset = 2;
        var offset = 2;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("String");
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 4,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = null,
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, semanticType, "this.Testificate", ref offset, new StringBuilder(), "", "");

        result.Should().BeEquivalentTo("bytes.WriteString(this.Testificate, offset + 2, 4);");
        offset.Should().Be(initialOffset);
    }

    [Theory]
    [InlineData("", 0, "", "", "this.Test.CopyTo(bytes, offset + 0);")]
    [InlineData("", 2, "", "", "this.Test.CopyTo(bytes, offset + 2);")]
    [InlineData("", 2, " + abc", "", "this.Test.CopyTo(bytes, offset + 2 + abc);")]
    [InlineData("", 2, " + abc", " + def", "this.Test.CopyTo(bytes, offset + 2 + abc + def);")]
    [InlineData("  ", 2, "", "", "  this.Test.CopyTo(bytes, offset + 2);")]
    public void GetLineForDynamicByteArray(string indentPrefix, int offset, string dynamicOffsetStart, string tempDynamicOffset, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForDynamicByteArray("this.Test", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        dynamicOffset.ToString().Should().BeEquivalentTo($"{dynamicOffsetStart} + this.Test.Length");
    }

    [Theory]
    [InlineData("0x0F", null, "            bytes[offset + 0] = 0x0F;")]
    [InlineData("0x0F", "0x20", "            bytes[offset + 0] = 0x0F;\r\n            bytes[offset + 1] = 0x20;")]
    public void GenerateWriteHeader(string header, string subHeader, string expected)
    {
        var result = SerializeGenerator.GenerateWriteHeader(header, subHeader);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(2, "", "", "", "bytes.WriteString(this.Testificate, offset + 2, (int)this.TestificateLength);")]
    [InlineData(2, "", "", "  ", "  bytes.WriteString(this.Testificate, offset + 2, (int)this.TestificateLength);")]
    [InlineData(2, " + abc", "", "", "bytes.WriteString(this.Testificate, offset + 2 + abc, (int)this.TestificateLength);")]
    [InlineData(2, " + abc", " + def", "", "bytes.WriteString(this.Testificate, offset + 2 + abc + def, (int)this.TestificateLength);")]
    [InlineData(4, "", "", "", "bytes.WriteString(this.Testificate, offset + 4, (int)this.TestificateLength);")]
    public void GetLineForString(int offset, string dynamicOffsetStart, string tempDynamicOffset, string indentPrefix, string expected)
    {
        var initialOffset = offset;
        var semanticType = Substitute.For<INamedTypeSymbol>();
        semanticType.Name.Returns("String");
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForString(new FieldData
        {
            Name = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            Order = null,
            SemanticType = semanticType,
            SyntaxNode = null,
            SizeFieldName = "TestificateLength",
            IsCustom = false,
            IsRecordParameter = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset);
        dynamicOffset.ToString().Should().BeEquivalentTo($"{dynamicOffsetStart} + this.Testificate.Length");
    }
}
