namespace QuantumCore.API.Game.Types.Monsters;

[Flags]
public enum EAiFlags
{
    Aggressive = 1 << 0,
    NoMove = 1 << 1,
    Coward = 1 << 2,
    NoAttackShinsoo = 1 << 3,
    NoAttackChunjo = 1 << 4,
    NoAttackJinno = 1 << 5,
    AttackMob = 1 << 6,
    Berserk = 1 << 7,
    StoneSkin = 1 << 8,
    GodSpeed = 1 << 9,
    DeathBlow = 1 << 10,
    Revive = 1 << 11,
}
