using QuantumCore.API;
using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World.Entities;

public abstract class Entity : IEntity
{
    private readonly IAnimationManager _animationManager;
    public uint Vid { get; }
    public EEmpire Empire { get; private protected set; }
    public abstract EEntityType Type { get; }
    public uint EntityClass { get; protected set; }
    public EEntityState State { get; protected set; }
    public virtual IEntity? Target { get; set; }

    public int PositionX
    {
        get => _positionX;
        set
        {
            _positionChanged = _positionChanged || _positionX != value;
            _positionX = value;
        }
    }

    public int PositionY
    {
        get => _positionY;
        set
        {
            _positionChanged = _positionChanged || _positionY != value;
            _positionY = value;
        }
    }

    public float Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    public bool PositionChanged
    {
        get => _positionChanged;
        set => _positionChanged = value;
    }

    public long Health { get; set; }
    public long Mana { get; set; }
    public abstract byte HealthPercentage { get; }
    public bool Dead { get; protected set; }

    public IMap? Map { get; set; }

    // QuadTree cache
    public int LastPositionX { get; set; }
    public int LastPositionY { get; set; }
    public IQuadTree? LastQuadTree { get; set; }

    // Movement related
    public long MovementStart { get; private set; }
    public int TargetPositionX { get; private set; }
    public int StartPositionX { get; private set; }
    public int TargetPositionY { get; private set; }
    public int StartPositionY { get; private set; }
    public uint MovementDuration { get; private set; }
    public byte MovementSpeed { get; set; }
    public byte AttackSpeed { get; set; }

    public IReadOnlyCollection<IEntity> NearbyEntities => _nearbyEntities;
    private readonly List<IEntity> _nearbyEntities = new();
    public List<IPlayerEntity> TargetedBy { get; } = new();
    public const int VIEW_DISTANCE = 10000;

    private int _positionX;
    private int _positionY;
    private float _rotation;
    private bool _positionChanged;
    protected PlayerEntity? LastAttacker { get; private set; }

    public Entity(IAnimationManager animationManager, uint vid)
    {
        _animationManager = animationManager;
        Vid = vid;
    }

    protected abstract void OnNewNearbyEntity(IEntity entity);
    protected abstract void OnRemoveNearbyEntity(IEntity entity);
    public abstract void OnDespawn();
    public abstract void ShowEntity(IConnection connection);
    public abstract void HideEntity(IConnection connection);

    public virtual void Update(double elapsedTime)
    {
        if (State == EEntityState.MOVING)
        {
            var elapsed = GameServer.Instance.ServerTime - MovementStart;
            var rate = MovementDuration == 0 ? 1 : elapsed / (float)MovementDuration;
            if (rate > 1) rate = 1;

            var x = (int)((TargetPositionX - StartPositionX) * rate + StartPositionX);
            var y = (int)((TargetPositionY - StartPositionY) * rate + StartPositionY);

            PositionX = x;
            PositionY = y;

            if (rate >= 1)
            {
                State = EEntityState.IDLE;
            }
        }
    }

    public virtual void Move(int x, int y)
    {
        if (PositionX == x && PositionY == y) return;

        PositionX = x;
        PositionY = y;
        PositionChanged = true;
    }

    public void Goto(Coordinates position) => Goto((int)position.X, (int)position.Y);

    public virtual void Goto(int x, int y)
    {
        if (PositionX == x && PositionY == y) return;
        if (TargetPositionX == x && TargetPositionY == y) return;

        var animation =
            _animationManager.GetAnimation(EntityClass, AnimationType.RUN, AnimationSubType.GENERAL);

        State = EEntityState.MOVING;
        TargetPositionX = x;
        TargetPositionY = y;
        StartPositionX = PositionX;
        StartPositionY = PositionY;
        MovementStart = GameServer.Instance.ServerTime;

        var distance = MathUtils.Distance(StartPositionX, StartPositionY, TargetPositionX, TargetPositionY);
        if (animation == null)
        {
            MovementDuration = 0;
        }
        else
        {
            var animationSpeed = -animation.AccumulationY / animation.MotionDuration;
            var i = 100 - MovementSpeed;
            if (i > 0)
            {
                i = 100 + i;
            }
            else if (i < 0)
            {
                i = 10000 / (100 - i);
            }
            else
            {
                i = 100;
            }

            var duration = (int)((distance / animationSpeed) * 1000) * i / 100;
            MovementDuration = (uint)duration;
        }
    }

    public virtual void Wait(int x, int y)
    {
        // todo: Verify position possibility
        PositionX = x;
        PositionY = y;
    }

    public void Stop()
    {
        State = EEntityState.IDLE;
        MovementDuration = 0;
    }

    public abstract EBattleType GetBattleType();
    public abstract int GetMinDamage();
    public abstract int GetMaxDamage();
    public abstract int GetBonusDamage();
    public abstract void AddPoint(EPoint point, int value);
    public abstract void SetPoint(EPoint point, uint value);
    public abstract uint GetPoint(EPoint point);

    public void Attack(IEntity victim)
    {
        if (this.PositionIsAttr(EMapAttributes.NON_PVP))
        {
            return;
        }

        if (victim.PositionIsAttr(EMapAttributes.NON_PVP))
        {
            return;
        }

        switch (GetBattleType())
        {
            case EBattleType.MELEE:
            case EBattleType.POWER:
            case EBattleType.TANKER:
            case EBattleType.SUPER_POWER:
            case EBattleType.SUPER_TANKER:
                // melee sort attack
                MeleeAttack(victim);
                break;
            case EBattleType.RANGE:

                RangeAttack(victim);
                break;
            case EBattleType.MAGIC:
                // todo magic attack
                break;
        }
    }

    private void MeleeAttack(IEntity victim)
    {
        // todo verify victim is in range

        var attackerRating = Math.Min(90, (GetPoint(EPoint.DX) * 4 + GetPoint(EPoint.LEVEL) * 2) / 6);
        var victimRating = Math.Min(90, (victim.GetPoint(EPoint.DX) * 4 + victim.GetPoint(EPoint.LEVEL) * 2) / 6);
        var attackRating = (attackerRating + 210.0) / 300.0 -
                           (victimRating * 2 + 5) / (victimRating + 95) * 3.0 / 10.0;

        var minDamage = GetMinDamage();
        var maxDamage = GetMaxDamage();

        var damage = CoreRandom.GenerateInt32(minDamage, maxDamage + 1) * 2;
        SendDebugDamage(victim, $"{this}->{victim} Base Attack value: {damage}");
        var attack = (int)(GetPoint(EPoint.ATTACK_GRADE) + damage - GetPoint(EPoint.LEVEL) * 2);
        attack = (int)Math.Floor(attack * attackRating);
        attack += (int)GetPoint(EPoint.LEVEL) * 2 + GetBonusDamage() * 2;
        attack *= (int)((100 + GetPoint(EPoint.ATTACK_BONUS) + GetPoint(EPoint.MAGIC_ATTACK_BONUS)) / 100);
        attack = CalculateAttackBonus(victim, attack);
        SendDebugDamage(victim, $"{this}->{victim} With bonus and level {attack}");

        var defence = (int)(victim.GetPoint(EPoint.DEFENCE_GRADE) * (100 + victim.GetPoint(EPoint.DEFENCE_BONUS)) /
                            100);
        SendDebugDamage(victim, $"{this}->{victim} Base defence: {defence}");
        if (this is MonsterEntity thisMonster)
        {
            attack = (int)Math.Floor(attack * thisMonster.Proto.DamageMultiply);
        }

        damage = Math.Max(0, attack - defence);
        SendDebugDamage(victim, $"{this}->{victim} Melee damage: {damage}");
        if (damage < 3)
        {
            damage = CoreRandom.GenerateInt32(1, 6);
        }

        // todo reduce damage by weapon type resist

        victim.Damage(this, EDamageType.NORMAL, damage);
    }

    private void RangeAttack(IEntity victim)
    {
        // todo verify victim is in range

        var attackerRating = Math.Min(90, (GetPoint(EPoint.DX) * 4 + GetPoint(EPoint.LEVEL) * 2) / 6);
        var victimRating = Math.Min(90, (victim.GetPoint(EPoint.DX) * 4 + victim.GetPoint(EPoint.LEVEL) * 2) / 6);
        var attackRating = (attackerRating + 210.0) / 300.0 -
                           (victimRating * 2 + 5) / (victimRating + 95) * 3.0 / 10.0;

        var minDamage = GetMinDamage();
        var maxDamage = GetMaxDamage();

        var damage = CoreRandom.GenerateInt32(minDamage, maxDamage + 1) * 2;
        var attack = (int)(GetPoint(EPoint.ATTACK_GRADE) + damage - GetPoint(EPoint.LEVEL) * 2);
        attack = (int)Math.Floor(attack * attackRating);
        attack += (int)GetPoint(EPoint.LEVEL) * 2 + GetBonusDamage() * 2;
        attack *= (int)((100 + GetPoint(EPoint.ATTACK_BONUS) + GetPoint(EPoint.MAGIC_ATTACK_BONUS)) / 100);
        attack = CalculateAttackBonus(victim, attack);

        var defence = (int)(victim.GetPoint(EPoint.DEFENCE_GRADE) * (100 + victim.GetPoint(EPoint.DEFENCE_BONUS)) /
                            100);
        if (this is MonsterEntity thisMonster)
        {
            attack = (int)Math.Floor(attack * thisMonster.Proto.DamageMultiply);
        }

        damage = Math.Max(0, attack - defence);
        if (damage < 3)
        {
            damage = CoreRandom.GenerateInt32(1, 6);
        }

        // todo reduce damage by weapon type resist

        foreach (var player in NearbyEntities.Where(x => x is IPlayerEntity).Cast<IPlayerEntity>())
        {
            player.Connection.Send(new ProjectilePacket
            {
                TargetX = victim.PositionX, TargetY = victim.PositionY, Target = victim.Vid, Shooter = Vid
            });
        }

        victim.Damage(this, EDamageType.NORMAL_RANGE, damage);
    }

    /// <summary>
    /// Adds bonus to the attack value for race bonus etc
    /// </summary>
    /// <param name="victim">The victim of the damage</param>
    /// <param name="attack">The current attack value</param>
    /// <returns>The new attack value with the bonus</returns>
    private int CalculateAttackBonus(IEntity victim, int attack)
    {
        // todo implement bonus attack against animals etc...
        // todo implement bonus attack against warriors etc...
        // todo implement resist again warriors etc...
        // todo implement resist against fire etc...

        return attack;
    }

    private int CalculateExperience(uint playerLevel)
    {
        var baseExp = GetPoint(EPoint.EXPERIENCE);
        var entityLevel = GetPoint(EPoint.LEVEL);

        var percentage = ExperienceConstants.GetExperiencePercentageByLevelDifference(playerLevel, entityLevel);

        return (int)(baseExp * percentage);
    }

    private void SendDebugDamage(IEntity other, string text)
    {
        if (this is PlayerEntity thisPlayer)
        {
            thisPlayer.SendChatInfo(text);
        }

        if (other is PlayerEntity otherPlayer)
        {
            otherPlayer.SendChatInfo(text);
        }
    }

    public virtual int Damage(IEntity attacker, EDamageType damageType, int damage)
    {

        if (this.PositionIsAttr(EMapAttributes.NON_PVP))
        {
            SendDebugDamage(attacker,
                $"{attacker}->{this} Ignoring damage inside NoPvP zone -> {damage} (should never happen)");
            return -1;
        }
            
        if (damageType is not EDamageType.NORMAL and not EDamageType.NORMAL_RANGE)
        {
            throw new NotImplementedException();
        }

        // todo block
        // todo handle berserk, fear, blessing skill
        // todo handle reflect melee

        SendDebugDamage(attacker, $"{attacker}->{this} Base Damage: {damage}");

        var isCritical = false;
        var isPenetrate = false;

        var criticalPercentage = attacker.GetPoint(EPoint.CRITICAL_PERCENTAGE);
        if (criticalPercentage > 0)
        {
            var resist = GetPoint(EPoint.RESIST_CRITICAL);
            criticalPercentage = resist > criticalPercentage ? 0 : criticalPercentage - resist;
            if (CoreRandom.PercentageCheck(criticalPercentage))
            {
                isCritical = true;
                damage *= 2;
                // todo send effect to clients
                SendDebugDamage(attacker,
                    $"{attacker}->{this} Critical hit -> {damage} (percentage was {criticalPercentage})");
            }
        }

        var penetratePercentage = attacker.GetPoint(EPoint.PENETRATE_PERCENTAGE);
        // todo add penetrate chance from passive
        if (penetratePercentage > 0)
        {
            var resist = GetPoint(EPoint.RESIST_PENETRATE);
            penetratePercentage = resist > penetratePercentage ? 0 : penetratePercentage - resist;
            if (CoreRandom.PercentageCheck(penetratePercentage))
            {
                isPenetrate = true;
                damage += (int)(GetPoint(EPoint.DEFENCE_GRADE) * (100 + GetPoint(EPoint.DEFENCE_BONUS)) / 100);
                SendDebugDamage(attacker,
                    $"{attacker}->{this} Penetrate hit -> {damage} (percentage was {penetratePercentage})");
            }
        }

        // todo calculate hp steal, sp steal, hp recovery, sp recovery and mana burn

        var damageFlags = EDamageFlags.NORMAL; // 1 = normal
        if (isCritical)
        {
            damageFlags |= EDamageFlags.CRITICAL;
        }

        if (isPenetrate)
        {
            damageFlags |= EDamageFlags.PIERCING;
        }

        var victimPlayer = this as PlayerEntity;
        var attackerPlayer = attacker as PlayerEntity;
        if (victimPlayer != null || attackerPlayer != null)
        {
            var damageInfo = new DamageInfo();
            damageInfo.Vid = Vid;
            damageInfo.Damage = damage;
            damageInfo.DamageFlags = damageFlags;

            if (victimPlayer != null)
            {
                victimPlayer.Connection.Send(damageInfo);
            }

            if (attackerPlayer != null)
            {
                attackerPlayer.Connection.Send(damageInfo);
                LastAttacker = attackerPlayer;
            }
        }

        this.Health -= damage;
        if (victimPlayer != null)
        {
            victimPlayer.SendPoints();
        }

        foreach (var playerEntity in TargetedBy)
        {
            playerEntity.SendTarget();
        }

        if (Health <= 0)
        {
            Die();
            if (Type != EEntityType.PLAYER && attackerPlayer is not null)
            {
                var exp = CalculateExperience(attackerPlayer.GetPoint(EPoint.LEVEL));
                attackerPlayer.AddPoint(EPoint.EXPERIENCE, exp);
                attackerPlayer.SendPoints();
            }
        }

        return damage;
    }

    public virtual void Die()
    {
        Dead = true;
    }

    public void AddNearbyEntity(IEntity entity)
    {
        _nearbyEntities.Add(entity);
        OnNewNearbyEntity(entity);
    }

    public void RemoveNearbyEntity(IEntity entity)
    {
        if (_nearbyEntities.Remove(entity))
        {
            OnRemoveNearbyEntity(entity);
        }
    }

    public void ForEachNearbyEntity(Action<IEntity> action)
    {
        foreach (var entity in _nearbyEntities)
        {
            action(entity);
        }
    }
}
