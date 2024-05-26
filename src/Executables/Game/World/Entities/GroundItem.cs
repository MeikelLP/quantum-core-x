﻿using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World.Entities;

public class GroundItem : Entity, IGroundItem
{
    private readonly ItemInstance _item;
    private readonly uint _amount;
    private readonly string? _ownerName;

    public ItemInstance Item => _item;
    public uint Amount => _amount;
    public string? OwnerName => _ownerName;

    public GroundItem(IAnimationManager animationManager, uint vid, ItemInstance item, uint amount,
        string? ownerName = null) : base(animationManager, vid)
    {
        _item = item;
        _amount = amount;
        _ownerName = ownerName;
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
        connection.Send(new GroundItemAdd(
            PositionX,
            PositionY,
            0,
            Vid,
            _item.ItemId
        ));
        connection.Send(new ItemOwnership
        {
            Vid = Vid,
            Player = OwnerName ?? ""
        });
    }

    public override void HideEntity(IConnection connection)
    {
        connection.Send(new GroundItemRemove(Vid));
    }

    public override uint GetPoint(EPoints point)
    {
        throw new System.NotImplementedException();
    }

    public override byte GetBattleType()
    {
        throw new System.NotImplementedException();
    }

    public override int GetMinDamage()
    {
        throw new System.NotImplementedException();
    }

    public override int GetMaxDamage()
    {
        throw new System.NotImplementedException();
    }

    public override int GetBonusDamage()
    {
        throw new System.NotImplementedException();
    }

    public override void AddPoint(EPoints point, int value)
    {
    }

    public override void SetPoint(EPoints point, uint value)
    {
    }
}