using BinarySerialization;

namespace QuantumCore.Models;

public class ItemApplyData
{
    [FieldOrder(1)] public byte Type { get; set; }
    [FieldOrder(2)] public uint Value { get; set; }
}