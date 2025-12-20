using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Systems.Events;

namespace QuantumCore.Game.World.Entities;

public class GroundItem : Entity, IGroundItem
{
    private readonly ItemInstance _item;
    private readonly uint _amount;
    private string? _ownerName;

    public ItemInstance Item => _item;
    public uint Amount => _amount;
    public string? OwnerName => _ownerName;

    private GroundItemEventRegistry Events { get; }
    protected override EntityEventRegistryBase BaseEvents => Events;


    public GroundItem(IAnimationManager animationManager, uint vid, ItemInstance item, uint amount,
        string? ownerName = null) : base(animationManager, vid)
    {
        _item = item;
        _amount = amount;
        _ownerName = ownerName;
        Events = new GroundItemEventRegistry(this);
        Events.Schedule(Events.OwnershipExpiry);
        Events.Schedule(Events.LifetimeExpiry);
    }

    public override EEntityType Type { get; }

    public override byte HealthPercentage { get; } = 0;

    protected override void OnNewNearbyEntity(IEntity entity)
    {
    }

    protected override void OnRemoveNearbyEntity(IEntity entity)
    {
    }

    public override void OnDespawn()
    {
    }

    public override void ShowEntity(IConnection connection)
    {
        connection.Send(new GroundItemAdd
        {
            PositionX = PositionX, PositionY = PositionY, Vid = Vid, ItemId = _item.ItemId
        });
        connection.Send(new ItemOwnership {Vid = Vid, Player = OwnerName ?? ""});
    }

    public override void HideEntity(IConnection connection)
    {
        connection.Send(new GroundItemRemove {Vid = Vid});
    }

    public bool ReleaseOwnership()
    {
        var hadOwner = _ownerName is not null;

        _ownerName = null;
        var clearOwnerPacket = new ItemOwnership { Vid = Vid, Player = "" };
        this.SafeBroadcastNearby(clearOwnerPacket);

        return hadOwner;
    }

    public override uint GetPoint(EPoint point)
    {
        throw new NotImplementedException();
    }

    public override EBattleType GetBattleType()
    {
        throw new NotImplementedException();
    }

    public override int GetMinDamage()
    {
        throw new NotImplementedException();
    }

    public override int GetMaxDamage()
    {
        throw new NotImplementedException();
    }

    public override int GetBonusDamage()
    {
        throw new NotImplementedException();
    }

    public override void AddPoint(EPoint point, int value)
    {
    }

    public override void SetPoint(EPoint point, uint value)
    {
    }
}
