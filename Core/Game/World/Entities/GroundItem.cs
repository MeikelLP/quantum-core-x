using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.World.Entities;

public class GroundItem : Entity, IGroundItem
{
    private readonly ItemInstance _item;
    private readonly uint _amount;

    public ItemInstance Item {
        get {
            return _item;
        }
    }

    public uint Amount {
        get {
            return _amount;
        }
    }

    public GroundItem(IAnimationManager animationManager, uint vid, ItemInstance item, uint amount) : base(animationManager, vid)
    {
        _item = item;
        _amount = amount;
    }

    public override EEntityType Type { get; }

    public override byte HealthPercentage { get; } = 0;
    
    protected override ValueTask OnNewNearbyEntity(IEntity entity)
    {
       return ValueTask.CompletedTask;
    }

    protected override ValueTask OnRemoveNearbyEntity(IEntity entity)
    {
        return ValueTask.CompletedTask;
    }

    public override ValueTask OnDespawn()
    {
        return ValueTask.CompletedTask;
    }

    public async override Task ShowEntity(IConnection connection)
    {
        await connection.Send(new GroundItemAdd {
            PositionX = PositionX,
            PositionY = PositionY,
            Vid = Vid,
            ItemId = _item.ItemId
        });
    }

    public async override Task HideEntity(IConnection connection)
    {
        await connection.Send(new GroundItemRemove {
            Vid = Vid
        });
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

    public override ValueTask AddPoint(EPoints point, int value)
    {
        throw new System.NotImplementedException();
    }

    public override ValueTask SetPoint(EPoints point, uint value)
    {
        throw new System.NotImplementedException();
    }
}