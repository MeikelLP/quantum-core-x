using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public sealed class ItemDataContainerPayload
{
    [FieldOrder(0), FieldLength(4), FieldEncoding("EUC-KR")]
    public string Header { get; set; } = "MCOZ";

    [FieldOrder(1)] public uint EncryptedSize { get; set; }
    [FieldOrder(2)] public uint DecryptedSize { get; set; }
    [FieldOrder(3)] public uint RealSize { get; set; }

    [FieldOrder(4), FieldLength(nameof(EncryptedSize))]
#pragma warning disable CA1819 // do not return arrays - required for BinarySerializer
    public byte[] EncryptedPayload { get; set; } = [];
#pragma warning restore CA1819
}