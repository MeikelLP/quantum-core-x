namespace QuantumCore.API.Core.Models;

public class MonsterData
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public string TranslatedName { get; set; } = "";
    public byte Type { get; set; }
    public byte Rank { get; set; }
    public byte BattleType { get; set; }
    public byte Level { get; set; }
    public byte Size { get; set; }
    public uint MinGold { get; set; }
    public uint MaxGold { get; set; }
    public uint Experience { get; set; }
    public uint Hp { get; set; }
    public byte RegenDelay { get; set; }
    public byte RegenPercentage { get; set; }
    public ushort Defence { get; set; }
    public uint AiFlag { get; set; }
    public uint RaceFlag { get; set; }
    public uint ImmuneFlag { get; set; }
    public byte St { get; set; }
    public byte Dx { get; set; }
    public byte Ht { get; set; }
    public byte Iq { get; set; }
    public List<uint> DamageRange { get; set; } = new();
    public short AttackSpeed { get; set; }
    public short MoveSpeed { get; set; }
    public byte AggressivePct { get; set; }
    public ushort AggressiveSight { get; set; }
    public ushort AttackRange { get; set; }
    public List<byte> Enchantments { get; set; } = new();
    public List<byte> Resists { get; set; } = new();
    public uint ResurrectionId { get; set; }
    public uint DropItemId { get; set; }
    public byte MountCapacity { get; set; }
    public byte OnClickType { get; set; }
    public byte Empire { get; set; }
    public string Folder { get; set; } = "";
    public float DamageMultiply { get; set; }
    public uint SummonId { get; set; }
    public uint DrainSp { get; set; }
    public uint MonsterColor { get; set; }
    public uint PolymorphItemId { get; set; }
    public List<MonsterSkillData> Skills { get; set; } = new();
    public byte BerserkPoint { get; set; }
    public byte StoneSkinPoint { get; set; }
    public byte GodSpeedPoint { get; set; }
    public byte DeathBlowPoint { get; set; }
    public byte RevivePoint { get; set; }
}
