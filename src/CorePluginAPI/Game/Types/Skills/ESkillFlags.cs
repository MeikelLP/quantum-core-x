namespace QuantumCore.API.Game.Types.Skills;

[Flags]
public enum ESkillFlags
{
    ATTACK = 1 << 0,
    USE_MELEE_DAMAGE = 1 << 1,
    COMPUTE_ATT_GRADE = 1 << 2,
    SELF_ONLY = 1 << 3,
    USE_MAGIC_DAMAGE = 1 << 4,
    USE_HP_AS_COST = 1 << 5,
    COMPUTE_MAGIC_DAMAGE = 1 << 6,
    SPLASH = 1 << 7,
    GIVE_PENALTY = 1 << 8,
    USE_ARROW_DAMAGE = 1 << 9,
    PENETRATE = 1 << 10,
    IGNORE_TARGET_RATING = 1 << 11,
    ATTACK_SLOW = 1 << 12,
    ATTACK_STUN = 1 << 13,
    HP_ABSORB = 1 << 14,
    SP_ABSORB = 1 << 15,
    ATTACK_FIRE_COUNT = 1 << 16,
    REMOVE_BAD_AFFECT = 1 << 17,
    REMOVE_GOOD_AFFECT  = 1 << 18,
    CRUSH = 1 << 19,
    ATTACK_POISON = 1 << 20,
    TOGGLE = 1 << 21,
    DISABLE_BY_POINT_UP = 1 << 22,
    CRUSH_LONG = 1 << 23,
    NONE = 0
}
