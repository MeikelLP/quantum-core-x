namespace QuantumCore.API.Game.Types;

[Flags]
public enum ERaceFlag : uint
{
    Animal = 1 << 0,
    Undead = 1 << 1,
    Devil = 1 << 2,
    Human = 1 << 3,
    Orc = 1 << 4,
    Milgyo = 1 << 5,
    Insect = 1 << 6,
    Fire = 1 << 7,
    Ice = 1 << 8,
    Desert = 1 << 9,
    Tree = 1 << 10,
    AttackElec = 1 << 11,
    AttackFire = 1 << 12,
    AttackIce = 1 << 13,
    AttackWind = 1 << 14,
    AttackEarth = 1 << 15,
    AttackDark = 1 << 16
}
