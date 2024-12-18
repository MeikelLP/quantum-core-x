using System.Diagnostics;
using BinarySerialization;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API.Core.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class MonsterData
{
    [FieldOrder(0)] public uint Id { get; set; }

    [FieldOrder(1)]
    [FieldLength(25), FieldEncoding("ASCII")]
    [SerializeAs(SerializedType.TerminatedString)]
    public string Name { get; set; } = "";

    [FieldOrder(2)]
    [FieldLength(25), FieldEncoding("ASCII")]
    [SerializeAs(SerializedType.TerminatedString)]
    public string TranslatedName { get; set; } = "";

    [FieldOrder(3)] public byte Type { get; set; }
    [FieldOrder(4)] public byte Rank { get; set; }
    [FieldOrder(5)] public EBattleType BattleType { get; set; }
    [FieldOrder(6)] public byte Level { get; set; }
    [FieldOrder(7)] public byte Size { get; set; }
    [FieldOrder(8)] public uint MinGold { get; set; }
    [FieldOrder(9)] public uint MaxGold { get; set; }
    [FieldOrder(10)] public uint Experience { get; set; }
    [FieldOrder(11)] public uint Hp { get; set; }
    [FieldOrder(12)] public byte RegenDelay { get; set; }
    [FieldOrder(13)] public byte RegenPercentage { get; set; }
    [FieldOrder(14)] public ushort Defence { get; set; }
    [FieldOrder(15)] public EAiFlags AiFlag { get; set; }
    [FieldOrder(16)] public uint RaceFlag { get; set; }
    [FieldOrder(17)] public uint ImmuneFlag { get; set; }
    [FieldOrder(18)] public byte St { get; set; }
    [FieldOrder(19)] public byte Dx { get; set; }
    [FieldOrder(20)] public byte Ht { get; set; }
    [FieldOrder(21)] public byte Iq { get; set; }
    [FieldOrder(22)] [FieldLength(4 * 2)] public List<uint> DamageRange { get; set; } = new();
    [FieldOrder(23)] public short AttackSpeed { get; set; }
    [FieldOrder(24)] public short MoveSpeed { get; set; }
    [FieldOrder(25)] public byte AggressivePct { get; set; }
    [FieldOrder(26)] public ushort AggressiveSight { get; set; }
    [FieldOrder(27)] public ushort AttackRange { get; set; }
    [FieldOrder(28)] [FieldLength(1 * 6)] public List<byte> Enchantments { get; set; } = new();
    [FieldOrder(29)] [FieldLength(1 * 11)] public List<byte> Resists { get; set; } = new();
    [FieldOrder(30)] public uint ResurrectionId { get; set; }
    [FieldOrder(31)] public uint DropItemId { get; set; }
    [FieldOrder(32)] public byte MountCapacity { get; set; }
    [FieldOrder(33)] public byte OnClickType { get; set; }
    [FieldOrder(34)] public byte Empire { get; set; }

    [FieldOrder(35)]
    [FieldLength(65), FieldEncoding("ASCII")]
    [SerializeAs(SerializedType.TerminatedString)]
    public string Folder { get; set; } = "";

    [FieldOrder(36)] public float DamageMultiply { get; set; }
    [FieldOrder(37)] public uint SummonId { get; set; }
    [FieldOrder(38)] public uint DrainSp { get; set; }
    [FieldOrder(39)] public uint MonsterColor { get; set; }
    [FieldOrder(40)] public uint PolymorphItemId { get; set; }
    [FieldOrder(41)] [FieldLength(5 * 5)] public List<MonsterSkillData> Skills { get; set; } = new();
    [FieldOrder(42)] public byte BerserkPoint { get; set; }
    [FieldOrder(43)] public byte StoneSkinPoint { get; set; }
    [FieldOrder(44)] public byte GodSpeedPoint { get; set; }
    [FieldOrder(45)] public byte DeathBlowPoint { get; set; }
    [FieldOrder(46)] public byte RevivePoint { get; set; }
}
