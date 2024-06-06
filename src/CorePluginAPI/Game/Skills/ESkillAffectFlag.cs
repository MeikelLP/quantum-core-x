using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

[Flags]
public enum ESkillAffectFlag
{
    [EnumMember(Value = "NONE")] None,
    [EnumMember(Value = "")] Ymir = 1 << 0,
    [EnumMember(Value = "INVISIBILITY")] Invisibility = 1 << 1,
    [EnumMember(Value = "SPAWN")] Spawn = 1 << 2,
    [EnumMember(Value = "POISON")] Poison = 1 << 3,
    [EnumMember(Value = "SLOW")] Slow = 1 << 4,
    [EnumMember(Value = "STUN")] Stun = 1 << 5,
    [EnumMember(Value = "DUNGEON_READY")] DungeonReady = 1 << 6,
    [EnumMember(Value = "FORCE_VISIBLE")] ForceVisible = 1 << 7,
    [EnumMember(Value = "BUILDING_CONSTRUCTION_SMALL")] BuildingConstructionSmall = 1 << 8,
    [EnumMember(Value = "BUILDING_CONSTRUCTION_LARGE")] BuildingConstructionLarge = 1 << 9,
    [EnumMember(Value = "BUILDING_UPGRADE")] BuildingUpgrade = 1 << 10,
    [EnumMember(Value = "MOV_SPEED_POTION")] MovementSpeedPotion = 1 << 11,
    [EnumMember(Value = "ATT_SPEED_POTION")] AttackSpeedPotion = 1 << 12,
    [EnumMember(Value = "FISH_MIDE")] FishMide = 1 << 13,
    [EnumMember(Value = "JEONGWIHON")] Jeongwihon = 1 << 14,
    [EnumMember(Value = "GEOMGYEONG")] Geomgyeong = 1 << 15,
    [EnumMember(Value = "CHEONGEUN")] Cheongeun = 1 << 16,
    [EnumMember(Value = "GYEONGGONG")] Gyeonggong = 1 << 17,
    [EnumMember(Value = "EUNHYUNG")] Eunhyung = 1 << 18,
    [EnumMember(Value = "GWIGUM")] Gwigum = 1 << 19,
    [EnumMember(Value = "TERROR")] Terror = 1 << 20,
    [EnumMember(Value = "JUMAGAP")] Jumagap = 1 << 21,
    [EnumMember(Value = "HOSIN")] Hosin = 1 << 22,
    [EnumMember(Value = "BOHO")] Boho = 1 << 23,
    [EnumMember(Value = "KWAESOK")] Kwaesok = 1 << 24,
    [EnumMember(Value = "MANASHIELD")] Manashield = 1 << 25,
    [EnumMember(Value = "MUYEONG")] Muyeong = 1 << 26,
    [EnumMember(Value = "REVIVE_INVISIBLE")] ReviveInvisible = 1 << 27,
    [EnumMember(Value = "FIRE")] Fire = 1 << 28,
    [EnumMember(Value = "GICHEON")] Gicheon = 1 << 29,
    [EnumMember(Value = "JEUNGRYEOK")] Jeungryeok = 1 << 30,
}
