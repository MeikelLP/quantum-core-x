using QuantumCore.API;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Systems.Events;

public sealed class GroundItemEventRegistry(IGroundItem item) : EntityEventRegistry<IGroundItem>(item)
{
    public OneShotEvent<IGroundItem> OwnershipExpiry { get; } = new(SchedulingConstants.GroundItemOwnershipLock,
        groundItem => groundItem.ReleaseOwnership());

    public OneShotEvent<IGroundItem> LifetimeExpiry { get; } = new(SchedulingConstants.GroundItemLifetime,
        groundItem => groundItem.Map?.DespawnEntity(groundItem));

    public override void Dispose()
    {
        Cancel(OwnershipExpiry);
        Cancel(LifetimeExpiry);
        base.Dispose();
    }
}
