using QuantumCore.API.Game.Types;

namespace QuantumCore.API;

public class Job
{
    public byte Ht { get; set; }
    public byte St { get; set; }
    public byte Dx { get; set; }
    public byte Iq { get; set; }
    public uint StartHp { get; set; }
    public uint StartSp { get; set; }
    public uint HpPerHt { get; set; }
    public uint SpPerIq { get; set; }
    public uint HpPerLevel { get; set; }
    public uint SpPerLevel { get; set; }
    public EPoints AttackStatus { get; set; }
}