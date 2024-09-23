using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public class ItemDataContainer
{
    [FieldOrder(0), FieldLength(4), FieldEncoding("EUC-KR")]
    public string Header { get; set; } = "MIPX";

    [FieldOrder(1)] public uint Version { get; set; }
    [FieldOrder(2)] public uint Stride { get; set; }
    [FieldOrder(3)] public uint Elements { get; set; }
    [FieldOrder(4)] public uint Size { get; set; }

    [FieldOrder(5)] public ItemDataContainerPayload Payload { get; set; } = new();
}