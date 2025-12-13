using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Types.Monsters;

public enum ESpawnPointType
{
    [EnumMember(Value = "g")] Group,
    [EnumMember(Value = "m")] Monster,
    [EnumMember(Value = "e")] Exception,
    [EnumMember(Value = "r")] GroupCollection,
    [EnumMember(Value = "s")] Special
}
