using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public class ItemDataContainerPayload
{
    [FieldOrder(5), FieldLength(4), FieldEncoding("EUC-KR")]
    public string Header { get; set; } = "MCOZ";

    [FieldOrder(6)] public uint EncryptedSize { get; set; }
    [FieldOrder(7)] public uint DecryptedSize { get; set; }
    [FieldOrder(8)] public uint RealSize { get; set; }

    [FieldOrder(9), FieldLength(nameof(EncryptedSize))]
    public byte[] EncryptedPayload { get; set; } = [];
}