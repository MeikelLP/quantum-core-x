using QuantumCore.API.Game.Types;

namespace QuantumCore.API.Core.Models;

public class Affect
{
    public Guid PlayerId { get; set; }
    public EAffectType Type { get; set; }
    public EApplyType ApplyOn { get; set; }
    public int ApplyValue { get; set; }
    public EAffects Flag { get; set; }
    public DateTime Duration { get; set; }
    public int SpCost { get; set; }
}
