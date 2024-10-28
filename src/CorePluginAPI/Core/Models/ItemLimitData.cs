using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public class ItemLimitData
{
    [FieldOrder(1)] public byte Type { get; set; }
    [FieldOrder(2)] public uint Value { get; set; }
}