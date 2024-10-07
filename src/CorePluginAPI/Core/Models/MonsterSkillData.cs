using BinarySerialization;

namespace QuantumCore.API.Core.Models;

public class MonsterSkillData
{
    [FieldOrder(0)] public uint Id { get; set; }
    [FieldOrder(1)] public byte Level { get; set; }
}
