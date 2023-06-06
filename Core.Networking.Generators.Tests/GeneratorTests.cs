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
    public void SimpleGeneratorTest()
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

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

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
            bytes[0] = 0xff;
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

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

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
            bytes[0] = 0xff;
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
            bytes[0] = 0xfd;
            bytes[offset + 1] = this.Phase;
        }

        public ushort GetSize() {
            return 2;
        }
    }
}");
    }

    [Fact]
    public void NonRecordStruct()
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

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

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
            bytes[0] = 0xff;
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
}