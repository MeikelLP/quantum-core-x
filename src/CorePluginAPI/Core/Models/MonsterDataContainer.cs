using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public class MonsterDataContainer
{
    [FieldOrder(0), FieldLength(4), FieldEncoding("EUC-KR")]
    public string Header { get; set; } = "MMPT";

    [FieldOrder(1)] public uint Elements { get; set; }
    [FieldOrder(2)] public uint Size { get; set; }

    [FieldOrder(3)] public MonsterDataContainerPayload Payload { get; set; } = new();
}