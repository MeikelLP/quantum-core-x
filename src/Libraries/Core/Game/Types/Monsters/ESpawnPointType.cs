using System.Runtime.Serialization;

namespace QuantumCore.Game.Types.Monsters;

public enum ESpawnPointType
{
    [EnumMember(Value = "g")] GROUP,
    [EnumMember(Value = "m")] MONSTER,
    [EnumMember(Value = "e")] EXCEPTION,
    [EnumMember(Value = "r")] GROUP_COLLECTION,
    [EnumMember(Value = "s")] SPECIAL
}
