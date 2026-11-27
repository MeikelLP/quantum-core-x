namespace QuantumCore.API.Game.Types.Monsters;

[Flags]
public enum ERaceFlags : uint
{
    ANIMAL = 1 << 0,
    UNDEAD = 1 << 1,
    DEVIL = 1 << 2,
    HUMAN = 1 << 3,
    ORC = 1 << 4,
    MILGYO = 1 << 5,
    INSECT = 1 << 6,
    FIRE = 1 << 7,
    ICE = 1 << 8,
    DESERT = 1 << 9,
    TREE = 1 << 10,
    ATTACK_ELEC = 1 << 11,
    ATTACK_FIRE = 1 << 12,
    ATTACK_ICE = 1 << 13,
    ATTACK_WIND = 1 << 14,
    ATTACK_EARTH = 1 << 15,
    ATTACK_DARK = 1 << 16
}
