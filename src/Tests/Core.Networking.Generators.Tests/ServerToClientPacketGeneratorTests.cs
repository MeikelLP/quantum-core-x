using System.Text;
using FluentAssertions;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class ServerToClientPacketGeneratorTests
{
    [Theory]
    [InlineData("", 0, "", "", "this.CopyTo(bytes, 0);")]
    [InlineData("", 2, "", "", "this.CopyTo(bytes, 2);")]
    [InlineData("", 2, " + abc", "", "this.CopyTo(bytes, 2 + abc);")]
    [InlineData("", 2, " + abc", " + def", "this.CopyTo(bytes, 2 + abc + def);")]
    [InlineData("  ", 2, " + abc", " + def", "  this.CopyTo(bytes, 2 + abc + def);")]
    public void GetLineForFixedByteArray(string indentPrefix, int offset, string dynamicOffsetStart,
        string tempDynamicOffset, string expected)
    {
        var initialOffset = offset;
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForFixedByteArray(new FieldData
        {
            FieldName = "Testificate",
            IsArray = false,
            IsEnum = false,
            ArrayLength = 2,
            ElementSize = 0,
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false,
        }, "this", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset + 2);
    }

    [Theory]
    [InlineData("System.Byte", "", 0, "", "", "bytes[0] = this.Testificate;")]
    [InlineData("System.Byte", "", 2, "", "", "bytes[2] = this.Testificate;")]
    [InlineData("System.Byte", "", 2, " + abc", "", "bytes[2 + abc] = this.Testificate;")]
    [InlineData("System.Byte", "", 2, " + abc", " + def", "bytes[2 + abc + def] = this.Testificate;")]
    [InlineData("System.Byte", "  ", 2, " + abc", " + def", "  bytes[2 + abc + def] = this.Testificate;")]
    [InlineData("System.Int16", "", 2, "", "", "bytes[2] = (System.Byte)(this.Testificate >> 0);\n" +
                                               "bytes[3] = (System.Byte)(this.Testificate >> 8);")]
    [InlineData("System.Int32", "", 2, "", "", "bytes[2] = (System.Byte)(this.Testificate >> 0);\n" +
                                               "bytes[3] = (System.Byte)(this.Testificate >> 8);\n" +
                                               "bytes[4] = (System.Byte)(this.Testificate >> 16);\n" +
                                               "bytes[5] = (System.Byte)(this.Testificate >> 24);")]
    public void GetLineForSingleValue(string type, string indentPrefix, int offset, string dynamicOffsetStart,
        string tempDynamicOffset, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = type,
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected.ReplaceLineEndings());
    }

    [Theory]
    [InlineData("System.Byte", "bytes[2] = (System.Byte)this.Testificate;")]
    [InlineData("System.Int16", "bytes[2] = (System.Byte)((System.Int16)this.Testificate >> 0);\n" +
                                "bytes[3] = (System.Byte)((System.Int16)this.Testificate >> 8);")]
    [InlineData("System.Int32", "bytes[2] = (System.Byte)((System.Int32)this.Testificate >> 0);\n" +
                                "bytes[3] = (System.Byte)((System.Int32)this.Testificate >> 8);\n" +
                                "bytes[4] = (System.Byte)((System.Int32)this.Testificate >> 16);\n" +
                                "bytes[5] = (System.Byte)((System.Int32)this.Testificate >> 24);")]
    public void GetLineForSingleValue_Enum(string underlyingType, string expected)
    {
        var offset = 2;
        var dynamicOffset = new StringBuilder();
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = true,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = "QuantumCore.EPhases",
            ElementTypeFullName = underlyingType,
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, dynamicOffset, "", "");

        result.Should().BeEquivalentTo(expected.ReplaceLineEndings());
    }

    [Fact]
    public void GetLineForSingleValue_String()
    {
        var initialOffset = 2;
        var offset = 2;
        var dynamicOffset = new StringBuilder();
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = "System.String",
            SizeFieldName = "TestificateLength",
            IsCustom = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, dynamicOffset, "", "");

        result.Should()
            .BeEquivalentTo("bytes.WriteString(this.Testificate, 2, (int)this.TestificateLength + 1);");
        offset.Should().Be(initialOffset);
    }

    [Fact]
    public void GetLineForSingleValue_FixedSizeString()
    {
        var initialOffset = 2;
        var offset = 2;
        var result = SerializeGenerator.GetLineForSingleValue(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 4,
            TypeFullName = "System.String",
            SizeFieldName = null,
            IsCustom = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, new StringBuilder(), "", "");

        result.Should().BeEquivalentTo("bytes.WriteString(this.Testificate, 2, 4);");
        offset.Should().Be(initialOffset);
    }

    [Theory]
    [InlineData("", 0, "", "", "this.Test.CopyTo(bytes, 0);")]
    [InlineData("", 2, "", "", "this.Test.CopyTo(bytes, 2);")]
    [InlineData("", 2, " + abc", "", "this.Test.CopyTo(bytes, 2 + abc);")]
    [InlineData("", 2, " + abc", " + def", "this.Test.CopyTo(bytes, 2 + abc + def);")]
    [InlineData("  ", 2, "", "", "  this.Test.CopyTo(bytes, 2);")]
    public void GetLineForDynamicByteArray(string indentPrefix, int offset, string dynamicOffsetStart,
        string tempDynamicOffset, string expected)
    {
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForDynamicByteArray("this.Test", ref offset, dynamicOffset,
            tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        dynamicOffset.ToString().Should().BeEquivalentTo($"{dynamicOffsetStart} + this.Test.Length");
    }

    [Theory]
    [InlineData((byte) 0x0F, null, "        bytes[0] = 0x0F;")]
    [InlineData((byte) 0x0F, (byte) 0x0F, "        bytes[0] = 0x0F;\n        bytes[1] = 0x0F;")]
    public void GenerateWriteHeader(byte header, byte? subHeader, string expected)
    {
        var result = SerializeGenerator.GenerateWriteHeader(header, subHeader);

        result.Should().BeEquivalentTo(expected.ReplaceLineEndings());
    }

    [Theory]
    [InlineData(2, "", "", "", "bytes.WriteString(this.Testificate, 2, (int)this.TestificateLength + 1);")]
    [InlineData(2, "", "", "  ", "  bytes.WriteString(this.Testificate, 2, (int)this.TestificateLength + 1);")]
    [InlineData(2, " + abc", "", "",
        "bytes.WriteString(this.Testificate, 2 + abc, (int)this.TestificateLength + 1);")]
    [InlineData(2, " + abc", " + def", "",
        "bytes.WriteString(this.Testificate, 2 + abc + def, (int)this.TestificateLength + 1);")]
    [InlineData(4, "", "", "", "bytes.WriteString(this.Testificate, 4, (int)this.TestificateLength + 1);")]
    public void GetLineForString(int offset, string dynamicOffsetStart, string tempDynamicOffset, string indentPrefix,
        string expected)
    {
        var initialOffset = offset;
        var dynamicOffset = new StringBuilder(dynamicOffsetStart);
        var result = SerializeGenerator.GetLineForString(new FieldData
        {
            FieldName = "",
            IsArray = false,
            IsEnum = false,
            ArrayLength = null,
            ElementSize = 0,
            TypeFullName = "System.String",
            SizeFieldName = "TestificateLength",
            IsCustom = false,
            IsReadonly = false,
        }, "this.Testificate", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);

        result.Should().BeEquivalentTo(expected);
        offset.Should().Be(initialOffset);
        dynamicOffset.ToString().Should().BeEquivalentTo($"{dynamicOffsetStart} + this.Testificate.Length");
    }
}