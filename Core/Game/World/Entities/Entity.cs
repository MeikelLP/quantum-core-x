using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World.Entities
{
    public abstract class Entity : IEntity
    {
        private readonly IAnimationManager _animationManager;
        public uint Vid { get; }
        public byte Empire { get; protected set; }
        public abstract EEntityType Type { get; }
        public uint EntityClass { get; protected set; }
        public EEntityState State { get; protected set; }
        public int PositionX
        {
            get => _positionX;
            set {
                _positionChanged = _positionChanged || _positionX != value;
                _positionX = value;
            }
        }
        public int PositionY
        {
            get => _positionY;
            set {
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
        
        public IMap Map { get; set; }
        
        // QuadTree cache
        public int LastPositionX { get; set; }
        public int LastPositionY { get; set; }
        public IQuadTree LastQuadTree { get; set; }
        
        // Movement related
        public long MovementStart { get; private set; }
        public int TargetPositionX { get; private set; }
        public int StartPositionX { get; private set; }
        public int TargetPositionY { get; private set; }
        public int StartPositionY { get; private set; }
        public uint MovementDuration { get; private set; }
        public byte MovementSpeed { get; protected set; }

        private List<IEntity> NearbyEntities { get; } = new();
        public List<IPlayerEntity> TargetedBy { get; } = new();
        public const int ViewDistance = 10000;

        private int _positionX;
        private int _positionY;
        private float _rotation;
        private bool _positionChanged;
        private IEntity _entityImplementation;

        public Entity(IAnimationManager animationManager, uint vid)
        {
            _animationManager = animationManager;
            Vid = vid;
            Empire = 0;
        }

        protected abstract ValueTask OnNewNearbyEntity(IEntity entity);
        protected abstract ValueTask OnRemoveNearbyEntity(IEntity entity);
        public abstract ValueTask OnDespawn();
        public abstract Task ShowEntity(IConnection connection);
        public abstract Task HideEntity(IConnection connection);
        
        public virtual Task Update(double elapsedTime)
        {
            if (State == EEntityState.Moving)
            {
                var elapsed = GameServer.Instance.ServerTime - MovementStart;
                var rate = MovementDuration == 0 ? 1 : elapsed / (float) MovementDuration;
                if (rate > 1) rate = 1;

                var x = (int)((TargetPositionX - StartPositionX) * rate + StartPositionX);
                var y = (int)((TargetPositionY - StartPositionY) * rate + StartPositionY);

                PositionX = x;
                PositionY = y;

                if (rate >= 1)
                {
                    State = EEntityState.Idle;
                }
            }

            return Task.CompletedTask;
        }

        public virtual Task Move(int x, int y)
        {
            if (PositionX == x && PositionY == y) return Task.CompletedTask;
            
            PositionX = x;
            PositionY = y;
            PositionChanged = true;
            return Task.CompletedTask;
        }

        public virtual Task Goto(int x, int y)
        {
            if (PositionX == x && PositionY == y) return Task.CompletedTask;
            if (TargetPositionX == x && TargetPositionY == y) return Task.CompletedTask;

            var animation =
                _animationManager.GetAnimation(EntityClass, AnimationType.Run, AnimationSubType.General);

            State = EEntityState.Moving;
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
                } else if (i < 0)
                {
                    i = 10000 / (100 - i);
                }
                else
                {
                    i = 100;
                }

                var duration = (int) ((distance / animationSpeed) * 1000) * i / 100;
                MovementDuration = (uint) duration;
            }
            
            return Task.CompletedTask;
        }

        public virtual void Wait(int x, int y)
        {
            // todo: Verify position possibility
            PositionX = x;
            PositionY = y;
        }

        public void Stop()
        {
            State = EEntityState.Idle;
            MovementDuration = 0;
        }

        public abstract byte GetBattleType();
        public abstract int GetMinDamage();
        public abstract int GetMaxDamage();
        public abstract int GetBonusDamage();
        public abstract ValueTask AddPoint(EPoints point, int value);
        public abstract ValueTask SetPoint(EPoints point, uint value);
        public abstract uint GetPoint(EPoints point);

        public async Task Attack(IEntity victim, byte type)
        {
            if (type == 0)
            {
                var battleType = GetBattleType();
                switch (battleType)
                {
                    case 0:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        // melee sort attack
                        await MeleeAttack(victim);
                        break;
                    case 1:
                        // todo range attack
                        break;
                    case 2:
                        // todo magic attack
                        break;
                }
            }
            else
            {
                // todo implement skills
            }
        }

        private async Task MeleeAttack(IEntity victim)
        {
            // todo verify victim is in range
            
            var attackerRating = Math.Min(90, (GetPoint(EPoints.Dx) * 4 + GetPoint(EPoints.Level) * 2) / 6);
            var victimRating = Math.Min(90, (victim.GetPoint(EPoints.Dx) * 4 + victim.GetPoint(EPoints.Level) * 2) / 6);
            var attackRating = (attackerRating + 210.0) / 300.0 - (victimRating * 2 + 5) / (victimRating + 95) * 3.0 / 10.0;

            var minDamage = GetMinDamage();
            var maxDamage = GetMaxDamage();

            var damage = CoreRandom.GenerateInt32(minDamage, maxDamage + 1) * 2;
            await SendDebugDamage(victim, $"{this}->{victim} Base Attack value: {damage}");
            var attack = (int)(GetPoint(EPoints.AttackGrade) + damage - GetPoint(EPoints.Level) * 2);
            attack = (int) Math.Floor(attack * attackRating);
            attack += (int)GetPoint(EPoints.Level) * 2 + GetBonusDamage() * 2;
            attack *= (int)((100 + GetPoint(EPoints.AttackBonus) + GetPoint(EPoints.MagicAttackBonus)) / 100);
            attack = CalculateAttackBonus(victim, attack);
            await SendDebugDamage(victim, $"{this}->{victim} With bonus and level {attack}");

            var defence = (int)(victim.GetPoint(EPoints.DefenceGrade) * (100 + victim.GetPoint(EPoints.DefenceBonus)) / 100);
            await SendDebugDamage(victim, $"{this}->{victim} Base defence: {defence}");
            if (this is MonsterEntity thisMonster)
            {
                attack = (int) Math.Floor(attack * thisMonster.Proto.DamageMultiply);
            }

            damage = Math.Max(0, attack - defence);
            await SendDebugDamage(victim, $"{this}->{victim} Melee damage: {damage}");
            if (damage < 3)
            {
                damage = CoreRandom.GenerateInt32(1, 6);
            }
            
            // todo reduce damage by weapon type resist
            
            await victim.Damage(this, EDamageType.Normal, damage);
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
            var baseExp = GetPoint(EPoints.Experience);
            var entityLevel = GetPoint(EPoints.Level);

            var percentage = ExperienceConstants.GetExperiencePercentageByLevelDifference(playerLevel, entityLevel);

            return (int)(baseExp * percentage);
        }

        private async Task SendDebugDamage(IEntity other, string text)
        {
            if (this is PlayerEntity thisPlayer)
            {
                await thisPlayer.SendChatInfo(text);
            }
            if (other is PlayerEntity otherPlayer)
            {
                await otherPlayer.SendChatInfo(text);
            }
        }

        public async virtual Task<int> Damage(IEntity attacker, EDamageType damageType, int damage)
        {
            if (damageType != EDamageType.Normal)
            {
                throw new NotImplementedException();
            }

            // todo block
            // todo handle berserk, fear, blessing skill
            // todo handle reflect melee
            
            await SendDebugDamage(attacker, $"{attacker}->{this} Base Damage: {damage}");

            var isCritical = false;
            var isPenetrate = false;

            var criticalPercentage = attacker.GetPoint(EPoints.CriticalPercentage);
            if (criticalPercentage > 0)
            {
                var resist = GetPoint(EPoints.ResistCritical);
                criticalPercentage = resist > criticalPercentage ? 0 : criticalPercentage - resist;
                if (CoreRandom.PercentageCheck(criticalPercentage))
                {
                    isCritical = true;
                    damage *= 2;
                    // todo send effect to clients
                    await SendDebugDamage(attacker, $"{attacker}->{this} Critical hit -> {damage} (percentage was {criticalPercentage})");
                }
            }

            var penetratePercentage = attacker.GetPoint(EPoints.PenetratePercentage);
            // todo add penetrate chance from passive
            if (penetratePercentage > 0)
            {
                var resist = GetPoint(EPoints.ResistPenetrate);
                penetratePercentage = resist > penetratePercentage ? 0 : penetratePercentage - resist;
                if(CoreRandom.PercentageCheck(penetratePercentage))
                {
                    isPenetrate = true;
                    damage += (int) (GetPoint(EPoints.DefenceGrade) * (100 + GetPoint(EPoints.DefenceBonus)) / 100);
                    await SendDebugDamage(attacker, $"{attacker}->{this} Penetrate hit -> {damage} (percentage was {penetratePercentage})");
                }
            } 
            
            // todo calculate hp steal, sp steal, hp recovery, sp recovery and mana burn

            byte damageFlags = 1; // 1 = normal
            if (isCritical)
            {
                damageFlags |= 32;
            }
            if (isPenetrate)
            {
                damageFlags |= 16;
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
                    await victimPlayer.Connection.Send(damageInfo);
                }

                if (attackerPlayer != null)
                {
                    await attackerPlayer.Connection.Send(damageInfo);
                }
            }

            this.Health -= damage;
            if (victimPlayer != null)
            {
                await victimPlayer.SendPoints();
            }

            foreach (var playerEntity in TargetedBy)
            {
                await playerEntity.SendTarget();
            }

            if (Health <= 0)
            {
                await Die();
                if (Type != EEntityType.Player && attackerPlayer is not null)
                {
                    var exp = CalculateExperience(attackerPlayer.GetPoint(EPoints.Level));
                    await attackerPlayer.AddPoint(EPoints.Experience, exp);
                    await attackerPlayer.SendPoints();
                }
            }

            return damage;
        }

        public virtual ValueTask Die()
        {
            Dead = true;
            return ValueTask.CompletedTask;
        }

        public async ValueTask AddNearbyEntity(IEntity entity)
        {
            NearbyEntities.Add(entity);
            await OnNewNearbyEntity(entity);
        }

        public async ValueTask RemoveNearbyEntity(IEntity entity)
        {
            if (NearbyEntities.Remove(entity))
            {
                await OnRemoveNearbyEntity(entity);
            }
        }

        public async Task ForEachNearbyEntity(Func<IEntity, Task> action)
        {
            foreach (var entity in NearbyEntities)
            {
                await action(entity);
            }
        }
    }
}