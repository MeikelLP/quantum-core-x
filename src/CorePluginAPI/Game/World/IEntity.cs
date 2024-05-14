using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API.Game.World
{
    public enum EEntityState
    {
        Idle,
        Moving
    }

    public enum EEntityType
    {
        Monster = 0,
        Npc = 1,
        MetinStone = 2,
        Player = 6
    }

    public interface IEntity
    {
        public uint Vid { get; }
        public uint EntityClass { get; }
        byte Empire { get; }
        public EEntityType Type { get; }
        public EEntityState State { get; }
        public bool PositionChanged { get; set; }
        public int PositionX { get; }
        public int PositionY { get; }
        public float Rotation { get; set; }
        public IMap? Map { get; set; }
        public byte HealthPercentage { get; }
        public List<IPlayerEntity> TargetedBy { get; }
        public bool Dead { get; }

        // QuadTree cache
        public int LastPositionX { get; set; }
        public int LastPositionY { get; set; }
        public IQuadTree? LastQuadTree { get; set; }

        // Movement related
        public long MovementStart { get; }
        public int TargetPositionX { get; }
        public int StartPositionX { get; }
        public int TargetPositionY { get; }
        public int StartPositionY { get; }
        public uint MovementDuration { get; }

        public void Update(double elapsedTime);

        public void OnDespawn();
        public void AddNearbyEntity(IEntity entity);
        public void RemoveNearbyEntity(IEntity entity);
        public void ForEachNearbyEntity(Action<IEntity> action);
        public void ShowEntity(IConnection connection);
        public void HideEntity(IConnection connection);
        IReadOnlyCollection<IEntity> NearbyEntities { get; }

        public uint GetPoint(EPoints point);
        public int GetMinDamage();
        public int GetMaxDamage();
        public int GetBonusDamage();

        public void Goto(int x, int y);
        public void Wait(int x, int y);

        public byte GetBattleType();
        public void Attack(IEntity victim, byte type);
        public int Damage(IEntity attacker, EDamageType damageType, int damage);

        public void Move(int x, int y);
        public void Stop();
        public void Die();
    }
}
