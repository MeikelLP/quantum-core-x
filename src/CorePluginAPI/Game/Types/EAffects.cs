using System;

namespace QuantumCore.API.Game.Types;

[Flags]
public enum EAffects : ulong
{
    None = 0,
    GameMaster = 1 << 0,
    Invisibility = 1 << 1,
    Spawn = 1 << 2,
    Poison = 1 << 3,
    Slow = 1 << 4,
    Stun = 1 << 5,
    MovSpeedPotion = 1 << 11,
    AttSpeedPotion = 1 << 12,
    ReviveInvisible = 1 << 27,
}
