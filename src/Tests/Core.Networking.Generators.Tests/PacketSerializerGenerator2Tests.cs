using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class PacketSerializerGenerator2Tests
{
    #region Helpers



    private static EquivalencyOptions<PacketTypeInfo> EqualityComparer (EquivalencyOptions<PacketTypeInfo> equality)
    {
        return equality.WithStrictOrdering();
    }

    private static PacketSerializerGenerator2 Compile(params string[] sources)
    {
        var compilation = CSharpCompilation.Create("compilation",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)), [
                MetadataReference.CreateFromFile(typeof(SerializerGeneratorTests).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Binder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ServerToClientPacketAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringLengthAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies()
                    .First(x => x.GetName().Name == "System.Runtime").Location)
            ], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new PacketSerializerGenerator2();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();

        return generator;
    }

    private static ImmutableArray<Diagnostic> CompileAndGetDiagnostics(params string[] sources)
    {
        var compilation = CSharpCompilation.Create("compilation",
            sources.Select(source => CSharpSyntaxTree.ParseText(source)), [
                MetadataReference.CreateFromFile(typeof(SerializerGeneratorTests).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Binder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ServerToClientPacketAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringLengthAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies()
                    .First(x => x.GetName().Name == "System.Runtime").Location)
            ], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new PacketSerializerGenerator2();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        return diagnostics;
    }
    #endregion

    // TODO custom types

    [Fact]
    public void ServerToClient()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                    [new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}],
                Header = 0x44,
                FixedSize = 5,
                IsServerToClient = true
            }
        ]);
    }

    [Fact]
    public void Array_Dynamic_NoSizeField()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public byte[] Data;
                           }
                           """;

        var diagnostics = CompileAndGetDiagnostics(src);
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().BeEquivalentTo(GeneratorCodes.DYNAMIC_REQUIRES_SIZE_FIELD);
    }

    [Fact]
    public void Array_Dynamic_ByConvention()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public ushort Size;
                               public byte[] Data;
                           }
                           """;


        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2},
                    new PacketFieldInfo {Name = "Data", TypeFullName = "System.Array", ElementSize = 1, IsArray = true}
                ],
                Header = 0x44,
                IsServerToClient = true,
                DynamicField = new PacketFieldInfo{Name = "Data", TypeFullName = "System.Array", ElementSize = 1, IsArray = true},
                DynamicSizeField = new PacketFieldInfo{Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2}
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Order()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public bool IsAggro;
                               [FieldOrder(0)] public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                    [
                        new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4, Order = 0},
                        new PacketFieldInfo {Name = "IsAggro", TypeFullName = typeof(bool).FullName!, ElementSize = 1}
                    ],
                Header = 0x44,
                FixedSize = 6,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Enum()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;
                           
                           public enum TestEnum
                           {
                               A
                           }

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public TestEnum Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields = [
                    new PacketFieldInfo
                    {
                        Name = "Id",
                        TypeFullName = "Quantum.Core.Networking.TestEnum",
                        ElementSize = 4,
                        IsEnum = true
                    }
                ],
                Header = 0x44,
                FixedSize = 5,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Enum_CustomSize()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;
                           
                           public enum TestEnum : ushort
                           {
                               A
                           }

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public TestEnum Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields = [
                    new PacketFieldInfo
                    {
                        Name = "Id",
                        TypeFullName = "Quantum.Core.Networking.TestEnum",
                        ElementSize = 2,
                        IsEnum = true
                    }
                ],
                Header = 0x44,
                FixedSize = 3,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Array_Fixed_Byte()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               [FixedSizeArray(4)]
                               public byte[] Data;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields = [
                    new PacketFieldInfo
                    {
                        Name = "Data",
                        TypeFullName = "System.Array",
                        ElementSize = 1,
                        ArrayLength = 4,
                        IsArray = true
                    }
                ],
                FixedSize = 5,
                Header = 0x44,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Array_Fixed_Int()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               [FixedSizeArray(2)]
                               public int[] Data;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields = [
                    new PacketFieldInfo
                    {
                        Name = "Data",
                        TypeFullName = "System.Array",
                        ElementSize = 4,
                        ArrayLength = 2,
                        IsArray = true
                    }
                ],
                FixedSize = 9,
                Header = 0x44,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void ClientToServer()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ClientToServerPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                    [new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}],
                Header = 0x44,
                FixedSize = 5,
                IsClientToServer = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void SubHeader()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44, 0x01)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                    [new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}],
                Header = 0x44,
                SubHeader = 0x01,
                FixedSize = 6,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Array_Dynamic_ByAttribute()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               [DynamicSizeFieldAttribute] public ushort CustomSize;
                               public byte[] Data;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "CustomSize", TypeFullName = typeof(ushort).FullName!, ElementSize = 2},
                    new PacketFieldInfo {Name = "Data", TypeFullName = "System.Array", ElementSize = 1, IsArray = true}
                ],
                Header = 0x44,
                IsServerToClient = true,
                DynamicField = new PacketFieldInfo {Name = "Data", TypeFullName = "System.Array", ElementSize = 1, IsArray = true},
                DynamicSizeField = new PacketFieldInfo
                {
                    Name = "CustomSize", TypeFullName = typeof(ushort).FullName!, ElementSize = 2
                }
            }
        ], EqualityComparer);
    }

    [Fact]
    public void String_Dynamic()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public ushort Size;
                               public string Message;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2},
                    new PacketFieldInfo {Name = "Message", TypeFullName = typeof(string).FullName!, ElementSize = 0}
                ],
                Header = 0x44,
                IsServerToClient = true,
                DynamicField = new PacketFieldInfo {Name = "Message", TypeFullName = typeof(string).FullName!, ElementSize = 0},
                DynamicSizeField = new PacketFieldInfo
                {
                    Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2
                }
            }
        ], EqualityComparer);
    }

    [Fact]
    public void String_Fixed()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               [FixedSizeStringAttribute(4)] public string Message;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Message", TypeFullName = typeof(string).FullName!, ElementSize = 4}
                ],
                FixedSize = 5,
                Header = 0x44,
                IsServerToClient = true,
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Dynamic_SizeFieldAfterDynamicField()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public byte[] Data;
                               [DynamicSizeFieldAttribute] public ushort CustomSize;
                           }
                           """;

        var diagnostics = CompileAndGetDiagnostics(src);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().BeEquivalentTo(GeneratorCodes.DYNAMIC_SIZE_FIELD_BEFORE_DYNAMIC_FIELD);
    }

    [Fact]
    public void Dynamic_Multiple()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                               public ushort Size;
                               public byte[] Data1;
                               public byte[] Data2;
                           }
                           """;

        var diagnostics = CompileAndGetDiagnostics(src);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().BeEquivalentTo(GeneratorCodes.DYNAMIC_FIELDS_MAX_ONCE);
    }

    [Fact]
    public void NoFields()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket
                           {
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields = [],
                Header = 0x44,
                FixedSize = 1,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Constant_ByReference()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           public class Codes
                           {
                               public const byte Header = 0x44;
                           }
                           
                           [ServerToClientPacket(Codes.Header)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}
                ],
                Header = 0x44,
                FixedSize = 5,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void Constant_Calculation()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;
                           
                           [ServerToClientPacket(0x22 + 0x22)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}
                ],
                Header = 0x44,
                FixedSize = 5,
                IsServerToClient = true
            }
        ], EqualityComparer);
    }

    [Fact]
    public void MultipleFiles()
    {
        const string src1 = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;
                           
                           [ServerToClientPacket(0x44)]
                           public partial struct TestPacket1
                           {
                               public uint Id;
                           }
                           """;
        const string src2 = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;
                           
                           [ServerToClientPacket(0x45)]
                           public partial struct TestPacket2
                           {
                               public uint Id;
                           }
                           """;

        Compile(src1, src2).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket1")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}
                ],
                Header = 0x44,
                FixedSize = 5,
                IsServerToClient = true
            },
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket2")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4}
                ],
                Header = 0x45,
                FixedSize = 5,
                IsServerToClient = true
            },
        ], EqualityComparer);
    }
}
