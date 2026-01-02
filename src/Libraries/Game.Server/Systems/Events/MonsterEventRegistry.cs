using QuantumCore.API.Game.World;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Systems.Events;

public sealed class MonsterEventRegistry(IEntity monster) : EntityEventRegistry<IEntity>(monster)
{
    public OneShotEvent<IEntity> DespawnAfterDeath { get; } = new(SchedulingConstants.MonsterDespawnAfterDeath,
        m => m.Map?.DespawnEntity(m));

    public override void Dispose()
    {
        Cancel(DespawnAfterDeath);
        base.Dispose();
    }
}
