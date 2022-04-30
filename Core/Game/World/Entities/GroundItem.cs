using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.World.Entities;

public class GroundItem : Entity
{
    private readonly Item _item;
    private readonly uint _amount;

    public Item Item {
        get {
            return _item;
        }
    }

    public uint Amount {
        get {
            return _amount;
        }
    }

    public GroundItem(uint vid, Item item, uint amount) : base(vid)
    {
        _item = item;
        _amount = amount;
    }

    public override EEntityType Type { get; }

    public override byte HealthPercentage { get; } = 0;
    
    protected override void OnNewNearbyEntity(IEntity entity)
    {
       
    }

    protected override void OnRemoveNearbyEntity(IEntity entity)
    {
        
    }

    public async override void OnDespawn()
    {
        
    }

    public override void ShowEntity(IConnection connection)
    {
        connection.Send(new GroundItemAdd {
            PositionX = PositionX,
            PositionY = PositionY,
            Vid = Vid,
            ItemId = _item.ItemId
        });
    }

    public override void HideEntity(IConnection connection)
    {
        connection.Send(new GroundItemRemove {
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

    public override void AddPoint(EPoints point, int value)
    {
        throw new System.NotImplementedException();
    }

    public override void SetPoint(EPoints point, uint value)
    {
        throw new System.NotImplementedException();
    }
}