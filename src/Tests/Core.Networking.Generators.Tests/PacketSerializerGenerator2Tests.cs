using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class PacketSerializerGenerator2Tests
{
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
                IsServerToClient = true
            }
        ]);
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
                IsClientToServer = true
            }
        ]);
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
                IsServerToClient = true
            }
        ]);
    }

    [Fact]
    public void Dynamic()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44, IsDynamic = true)]
                           public partial struct TestPacket
                           {
                               public uint Id;
                               public ushort Size;
                           }
                           """;

        Compile(src).PacketTypes.Should().BeEquivalentTo([
            new PacketTypeInfo("Quantum.Core.Networking", "TestPacket")
            {
                Fields =
                [
                    new PacketFieldInfo {Name = "Id", TypeFullName = typeof(uint).FullName!, ElementSize = 4},
                    new PacketFieldInfo {Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2}
                ],
                Header = 0x44,
                HasDynamicLength = true,
                IsServerToClient = true,
                DynamicSizeField = new PacketFieldInfo
                {
                    Name = "Size", TypeFullName = typeof(ushort).FullName!, ElementSize = 2
                }
            }
        ]);
    }

    [Fact]
    public void Dynamic_MissingField()
    {
        const string src = """
                           using QuantumCore.Networking;

                           namespace Quantum.Core.Networking;

                           [ServerToClientPacket(0x44, IsDynamic = true)]
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
                HasDynamicLength = true,
                IsServerToClient = true
            }
        ]);
    }
}
