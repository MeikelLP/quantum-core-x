using BinarySerialization;

namespace QuantumCore.Models;

public class ItemLimitData
{
    [FieldOrder(1)] public byte Type { get; set; }
    [FieldOrder(2)] public uint Value { get; set; }
}