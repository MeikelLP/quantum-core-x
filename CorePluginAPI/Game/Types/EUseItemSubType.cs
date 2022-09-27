namespace QuantumCore.API.Game.Types;

public enum EUseItemSubType
{
    USE_POTION = 0,
    USE_TALISMAN = 1,
    USE_TUNING = 2,
    USE_MOVE = 3,
    USE_TREASURE_BOX = 4,
    USE_MONEYBAG = 5,
    USE_BAIT = 6,
    USE_ABILITY_UP = 7,
    USE_AFFECT = 8,
    USE_CREATE_STONE = 9,
    USE_SPECIAL = 10,
    USE_POTION_NODELAY = 11,
    USE_CLEAR = 12,
    USE_INVISIBILITY = 13,
    USE_DETACHMENT = 14,
    USE_BUCKET = 15,
    USE_POTION_CONTINUE = 16,
    USE_CLEAN_SOCKET = 17,
    USE_CHANGE_ATTRIBUTE = 18,
    USE_ADD_ATTRIBUTE = 19,
    USE_ADD_ACCESSORY_SOCKET = 20,
    USE_PUT_INTO_ACCESSORY_SOCKET = 21,
    USE_ADD_ATTRIBUTE2 = 22,
    USE_RECIPE = 23,
    USE_CHANGE_ATTRIBUTE2 = 24,
    USE_BIND = 25,
    USE_UNBIND = 26,
    USE_TIME_CHARGE_PER = 27,
    USE_TIME_CHARGE_FIX = 28,

    /// <summary>
    /// items that can be used for belt sockets
    /// </summary>
    USE_PUT_INTO_BELT_SOCKET = 29,

    /// <summary>
    /// items that can be used for ring sockets (not unique rings, newly added ring slots)
    /// </summary>
    USE_PUT_INTO_RING_SOCKET = 30,
}