using System.Collections.Immutable;
using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public sealed class MonsterDataContainerPayload
{
    [FieldOrder(0), FieldLength(4), FieldEncoding("EUC-KR")]
    public string Header { get; set; } = "MCOZ";

    [FieldOrder(1)] public uint EncryptedSize { get; set; }
    [FieldOrder(2)] public uint DecryptedSize { get; set; }
    [FieldOrder(3)] public uint RealSize { get; set; }

    [FieldOrder(4), FieldLength(nameof(EncryptedSize))]
    public ImmutableArray<byte> EncryptedPayload { get; set; } = [];
}