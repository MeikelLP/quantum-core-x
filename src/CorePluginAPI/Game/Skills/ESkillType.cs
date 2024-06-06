using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

public enum ESkillType
{
    [EnumMember(Value = "NORMAL")] Normal,
    [EnumMember(Value = "MELEE")] Melee,
    [EnumMember(Value = "RANGE")] Range,
    [EnumMember(Value = "MAGIC")] Magic
}
