using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.API.Game.World;

public interface IEntity
{
    uint Vid { get; }
    uint EntityClass { get; }
    EEmpire Empire { get; }
    long Health { get; set; }
    EEntityType Type { get; }
    EEntityState State { get; }
    bool PositionChanged { get; set; }
    int PositionX { get; }
    int PositionY { get; }
    float Rotation { get; set; }
    IMap? Map { get; set; }
    byte HealthPercentage { get; }
    IEntity? Target { get; set; }
#pragma warning disable CA1002 // do not expose lists - is ok for now
    List<IPlayerEntity> TargetedBy { get; }
#pragma warning restore CA1002
    bool Dead { get; }

    // QuadTree cache
    int LastPositionX { get; set; }
    int LastPositionY { get; set; }
    IQuadTree? LastQuadTree { get; set; }

    // Movement related
    ServerTimestamp MovementStart { get; }
    int TargetPositionX { get; }
    int StartPositionX { get; }
    int TargetPositionY { get; }
    int StartPositionY { get; }
    uint MovementDuration { get; }

    void Update(TickContext ctx);

    void OnDespawn();
    void AddNearbyEntity(IEntity entity);
    void RemoveNearbyEntity(IEntity entity);
    void ForEachNearbyEntity(Action<IEntity> action);
    void ShowEntity(IConnection connection);
    void HideEntity(IConnection connection);
    IReadOnlyCollection<IEntity> NearbyEntities { get; }
    byte MovementSpeed { get; set; }
    byte AttackSpeed { get; set; }

    uint GetPoint(EPoint point);
    int GetMinDamage();
    int GetMaxDamage();
    int GetBonusDamage();

    void Goto(int x, int y, ServerTimestamp startAt);
    void Wait(int x, int y);

    void Attack(IEntity victim);
    int Damage(IEntity attacker, EDamageType damageType, int damage);

    void Move(int x, int y);
    void Stop();
    void Die();
}