﻿using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using QuantumCore.Core.Networking;
using QuantumCore.Networking;
using Xunit;

namespace Core.Networking.Generators.Tests;

public class GeneratorTests
{
    private static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(GeneratorTests).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Binder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(PacketAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies()
                    .First(x => x.GetName().Name == "System.Runtime").Location),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    [Fact]
    public void RecordStruct()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(uint Handshake, uint Time, uint Delta);
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void Record()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record GCHandshake(uint Handshake, uint Time, uint Delta);
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void Record_CustomOrder()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(uint Handshake, uint Time) {
    [Field(0)]
    public uint Size => 15; 
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Size).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void Record_WithDynamicString()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(byte Type, string Message) {
    [Field(1)]
    public uint Size => (uint)Message.Length;
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = this.Type;
            System.BitConverter.GetBytes(this.Size).CopyTo(bytes, offset + 2);
            System.Text.Encoding.ASCII.GetBytes(this.Message).CopyTo(bytes, offset + 6);
        }

        public ushort GetSize() {
            return (ushort)(6 + this.Message.Length);
        }
    }
}");
    }

    [Fact]
    public void Record_WithDynamicByteArray()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(byte Type, byte[] Flags) {
    [Field(1)]
    public uint Size => (uint)Flags.Length;
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = this.Type;
            System.BitConverter.GetBytes(this.Size).CopyTo(bytes, offset + 2);
            this.Flags.CopyTo(bytes, offset + 6);
        }

        public ushort GetSize() {
            return (ushort)(6 + this.Flags.Length);
        }
    }
}");
    }
    
    [Fact]
    public void Record_WithFixedByteArray()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(byte Type)
{
    public byte[] Flags { get; init; } = new byte[4];
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = this.Type;
            this.Flags.CopyTo(bytes, offset + 2);
        }

        public ushort GetSize() {
            return 6;
        }
    }
}");
    }

    [Fact]
    public void Record_WithDynamic_FieldAfter()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(byte Type, string Message, byte Location) {
    [Field(1)]
    public uint Size => (uint)this.Message.Length;
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = this.Type;
            System.BitConverter.GetBytes(this.Size).CopyTo(bytes, offset + 2);
            System.Text.Encoding.ASCII.GetBytes(this.Message).CopyTo(bytes, offset + 6);
            bytes[offset + 6 + this.Message.Length] = this.Location;
        }

        public ushort GetSize() {
            return (ushort)(7 + this.Message.Length);
        }
    }
}");
    }

    [Fact]
    public void MultipleTypesPerFile()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCHandshake(uint Handshake, uint Time, uint Delta);

[Packet(0xfd, EDirection.Outgoing)]
[PacketGenerator]
public partial record struct GCPhase(byte Phase);
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(2);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(2);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
        runResult.Results[0].GeneratedSources[1].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record struct GCPhase : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xfd;
            bytes[offset + 1] = this.Phase;
        }

        public ushort GetSize() {
            return 2;
        }
    }
}");
    }

    [Fact]
    public void Struct()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial struct GCHandshake {
    public uint Handshake { get; set; }
    public uint Time { get; set; }
    public uint Delta { get; set; }
}");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial struct GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void Class()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial class GCHandshake {
    public uint Handshake { get; set; }
    public uint Time { get; set; }
    public uint Delta { get; set; }
}");

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial class GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void RecordWithMembers()
    {
        var inputCompilation = CreateCompilation(@"
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets;

[Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
[PacketGenerator]
public partial record GCHandshake(uint Handshake, uint Time) {
    public uint Delta { get; init; }
}
".Trim());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics().Should().BeEmpty();

        runResult.GeneratedTrees.Should().HaveCount(1);
        runResult.Diagnostics.Should().BeEmpty();

        runResult.Results[0].GeneratedSources.Should().HaveCount(1);
        runResult.Results[0].Diagnostics.Should().BeEmpty();
        runResult.Results[0].Exception.Should().BeNull();
        runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
using QuantumCore.Networking;

namespace QuantumCore.Core.Packets {

    public partial record GCHandshake : IPacketSerializable
    {
        public void Serialize(byte[] bytes, int offset = 0) {
            bytes[offset + 0] = 0xff;
            System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
            System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
            System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
        }

        public ushort GetSize() {
            return 13;
        }
    }
}");
    }

    [Fact]
    public void ReadonlyRefStruct()
    {
        Assert.True(true, "ref structs cannot implement interfaces");
    }

//     [Fact]
//     public void DynamicWith_String()
//     {
//         var inputCompilation = CreateCompilation(@"
// using QuantumCore.Core.Networking;
// using QuantumCore.Networking;
//
// namespace QuantumCore.Core.Packets;
//
// [Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
// [PacketGenerator]
// public partial record struct GCHandshake(uint Handshake, uint Time, uint Delta);
// ".Trim());
//
//         GeneratorDriver driver = CSharpGeneratorDriver.Create(new SerializerGenerator());
//
//         driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
//             out var diagnostics);
//
//         var runResult = driver.GetRunResult();
//         diagnostics.Should().BeEmpty();
//         outputCompilation.GetDiagnostics().Should().BeEmpty();
//
//         runResult.GeneratedTrees.Should().HaveCount(1);
//         runResult.Diagnostics.Should().BeEmpty();
//
//         runResult.Results[0].GeneratedSources.Should().HaveCount(1);
//         runResult.Results[0].Diagnostics.Should().BeEmpty();
//         runResult.Results[0].Exception.Should().BeNull();
//         runResult.Results[0].GeneratedSources[0].SourceText.ToString().Should().BeEquivalentTo(@"/// <auto-generated/>
// using QuantumCore.Networking;
//
// namespace QuantumCore.Core.Packets {
//
//     public partial record struct GCHandshake : IPacketSerializable
//     {
//         public void Serialize(byte[] bytes, int offset = 0) {
//             bytes[offset + 0] = 0xff;
//             System.BitConverter.GetBytes(this.Handshake).CopyTo(bytes, offset + 1);
//             System.BitConverter.GetBytes(this.Time).CopyTo(bytes, offset + 5);
//             System.BitConverter.GetBytes(this.Delta).CopyTo(bytes, offset + 9);
//         }
//
//         public ushort GetSize() {
//             return 13;
//         }
//     }
// }");
}