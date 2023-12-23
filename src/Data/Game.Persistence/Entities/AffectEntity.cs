using Dapper.Contrib.Extensions;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Persistence.Entities;

[Table("affects")]
internal class AffectEntity
{
    [ExplicitKey]
    public Guid PlayerId { get; set; }
    [ExplicitKey]
    public EAffectType Type { get; set; }
    [ExplicitKey]
    public EApplyType ApplyOn { get; set; }
    [ExplicitKey]
    public int ApplyValue { get; set; }
    public EAffects Flag { get; set; }
    public DateTime Duration { get; set; }
    public int SpCost { get; set; }
}
