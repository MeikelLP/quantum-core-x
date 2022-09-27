using System;

namespace QuantumCore.Game;

[Flags]
public enum EItemFlags
{
    ITEM_FLAG_REFINEABLE = 1 << 0,
    ITEM_FLAG_SAVE = 1 << 1,
    ITEM_FLAG_STACKABLE = 1 << 2,
    ITEM_FLAG_COUNT_PER_1GOLD = 1 << 3,
    ITEM_FLAG_SLOW_QUERY = 1 << 4,
    ITEM_FLAG_UNUSED01 = 1 << 5,
    ITEM_FLAG_UNIQUE = 1 << 6,
    ITEM_FLAG_MAKECOUNT = 1 << 7,
    ITEM_FLAG_IRREMOVABLE = 1 << 8,
    ITEM_FLAG_CONFIRM_WHEN_USE = 1 << 9,
    ITEM_FLAG_QUEST_USE = 1 << 10,
    ITEM_FLAG_QUEST_USE_MULTIPLE = 1 << 11,
    ITEM_FLAG_QUEST_GIVE = 1 << 12,
    ITEM_FLAG_LOG = 1 << 13,
    ITEM_FLAG_APPLICABLE = 1 << 14,
}