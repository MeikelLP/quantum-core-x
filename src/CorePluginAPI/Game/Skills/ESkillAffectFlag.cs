﻿using System.Runtime.Serialization;

namespace QuantumCore.API.Game.Skills;

[Flags]
public enum ESkillAffectFlag
{
    None,
    Ymir = 1 << 0,
    Invisibility = 1 << 1,
    Spawn = 1 << 2,
    Poison = 1 << 3,
    Slow = 1 << 4,
    Stun = 1 << 5,
    DungeonReady = 1 << 6,
    ForceVisible = 1 << 7,
    BuildingConstructionSmall = 1 << 8,
    BuildingConstructionLarge = 1 << 9,
    BuildingUpgrade = 1 << 10,
    MovementSpeedPotion = 1 << 11,
    AttackSpeedPotion = 1 << 12,
    FishMide = 1 << 13,
    Jeongwihon = 1 << 14,
    Geomgyeong = 1 << 15,
    Cheongeun = 1 << 16,
    Gyeonggong = 1 << 17,
    Eunhyung = 1 << 18,
    Gwigum = 1 << 19,
    Terror = 1 << 20,
    Jumagap = 1 << 21,
    Hosin = 1 << 22,
    Boho = 1 << 23,
    Kwaesok = 1 << 24,
    Manashield = 1 << 25,
    Muyeong = 1 << 26,
    ReviveInvisible = 1 << 27,
    Fire = 1 << 28,
    Gicheon = 1 << 29,
    Jeungryeok = 1 << 30,
}
