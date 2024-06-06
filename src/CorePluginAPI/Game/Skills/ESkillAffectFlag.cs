using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

public enum ESkillAffectFlag
{
    [EnumMember(Value = "")] Ymir,
    [EnumMember(Value = "INVISIBILITY")] Invisibility,
    [EnumMember(Value = "SPAWN")] Spawn,
    [EnumMember(Value = "POISON")] Poison,
    [EnumMember(Value = "SLOW")] Slow,
    [EnumMember(Value = "STUN")] Stun,
    [EnumMember(Value = "DUNGEON_READY")] DungeonReady,
    [EnumMember(Value = "FORCE_VISIBLE")] ForceVisible,
    [EnumMember(Value = "BUILDING_CONSTRUCTION_SMALL")] BuildingConstructionSmall,
    [EnumMember(Value = "BUILDING_CONSTRUCTION_LARGE")] BuildingConstructionLarge,
    [EnumMember(Value = "BUILDING_UPGRADE")] BuildingUpgrade,
    [EnumMember(Value = "MOV_SPEED_POTION")] MovementSpeedPotion,
    [EnumMember(Value = "ATT_SPEED_POTION")] AttackSpeedPotion,
    [EnumMember(Value = "FISH_MIDE")] FishMide,
    [EnumMember(Value = "JEONGWIHON")] Jeongwihon,
    [EnumMember(Value = "GEOMGYEONG")] Geomgyeong,
    [EnumMember(Value = "CHEONGEUN")] Cheongeun,
    [EnumMember(Value = "GYEONGGONG")] Gyeonggong,
    [EnumMember(Value = "EUNHYUNG")] Eunhyung,
    [EnumMember(Value = "GWIGUM")] Gwigum,
    [EnumMember(Value = "TERROR")] Terror,
    [EnumMember(Value = "JUMAGAP")] Jumagap,
    [EnumMember(Value = "HOSIN")] Hosin,
    [EnumMember(Value = "BOHO")] Boho,
    [EnumMember(Value = "KWAESOK")] Kwaesok,
    [EnumMember(Value = "MANASHIELD")] Manashield,
    [EnumMember(Value = "MUYEONG")] Muyeong,
    [EnumMember(Value = "REVIVE_INVISIBLE")] ReviveInvisible,
    [EnumMember(Value = "FIRE")] Fire,
    [EnumMember(Value = "GICHEON")] Gicheon,
    [EnumMember(Value = "JEUNGRYEOK")] Jeungryeok
}
