﻿/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:51 a.m.
 *
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    ///     Description of Class1.
    /// </summary>
    public delegate void OnAttackEventHandler(IEntity Victim, float Amount);

    public class Entity : IRenderable, IDisposable, ISearchable, IEntity
    {
        private DamageComponent _damageManager;
        private int _drowningSoundTimer;
        private float _health = 100f;
        private int _isHumanoid = -1;
        private bool _knocked;
        private string _name;
        private float _oxygen = 30;
        private float _prevHeight;
        private bool _spawningWithAnimation;
        private float _knockedTime;
        private bool _isUnderwater;
        private Timer _splashTimer;
        private readonly TickSystem _tickSystem;
        public PhysicsComponent Physics { get; }

        protected List<IComponent<IEntity>> Components = new List<IComponent<IEntity>>();
        protected bool Splashed { get; set; }

        public event OnAttackEventHandler OnAttacking;
        public event OnAttackEventHandler BeforeAttacking;
        public EntityComponentManager ComponentManager { get; }
        public float AttackDamage { get; set; } = 1.0f;
        public float AttackCooldown { get; set; }
        public virtual float AttackResistance { get; set; } = 1.0f;
        public bool Destroy { get; set; } = false;
        public int Level { get; set; } = 1;
        public float MaxOxygen { get; set; } = 30;
        public int MobId { get; set; }
        public int MobSeed { get; set; }
        public Vector3 Orientation { get; set; } = Vector3.UnitZ;       
        public bool Removable { get; set; } = true;
        public Vector3 BlockPosition { get; set; }
        public bool PlaySpawningAnimation { get; set; } = true;
        public float Speed { get; set; } = 2;
        public bool IsMoving => Model.IsMoving;

        public virtual float Health
        {
            get => _health;
            set
            {
                if (value < 0)
                {
                    IsDead = true;
                    this.Dispose();
                }
                _health = Math.Min(value, MaxHealth);
            }
        }

        public bool InUpdateRange => (BlockPosition.Xz - LocalPlayer.Instance.Model.Position.Xz).LengthSquared <
                                     GameSettings.UpdateDistance * GameSettings.UpdateDistance;

        public bool IsActive { get; set; }
        public bool IsBoss { get; set; }
        public bool IsDead { get; set; }
        public bool IsFlying { get; set; }

        public bool IsFriendly
        {
            get
            {
                if (this.SearchComponent<WarriorAIComponent>() != null)
                    return this.SearchComponent<WarriorAIComponent>().Friendly;
                return this.SearchComponent<VillagerAIComponent>() != null;
            }
        }

        public bool IsGrounded { get; set; }

        public bool IsHumanoid
        {
            get
            {
                if (_isHumanoid == -1)
                    _isHumanoid = this is Humanoid ? 1 : 0;
                return _isHumanoid == 1;
            }
        }

        public bool IsImmune
        {
            get
            {
                var dmg = this.SearchComponent<DamageComponent>();
                return dmg != null && dmg.Immune;
            }
        }

        public bool IsInvisible { get; set; }
        public bool IsStatic => Model.IsStatic;

        public bool IsKnocked
        {
            get => _knocked;
            set
            {
                if (value == _knocked) return;
                _knocked = value;
                this.ShowIcon(_knocked ? (CacheItem?) CacheItem.KnockedIcon : null);
            }
        }

        public virtual float MaxHealth { get; set; } = 100f;

        public MobType MobType
        {
            get
            {
                bool result = Enum.TryParse(Type, true, out MobType enumeration);
                return result ? enumeration : MobType.Unknown;
            }

            set
            {
                if (value != MobType.Unknown)
                    Type = value.ToString();
            }
        }

        public BaseUpdatableModel Model { get; set; }

        public string Name
        {
            get
            {
                if (_name != null) return _name;

                var bar = this.SearchComponent<HealthBarComponent>();
                var bossHealthBar = this.SearchComponent<BossHealthBarComponent>();
                if (bar != null)
                    return bar.Name;

                return bossHealthBar?.Name ?? "null";
            }
            set
            {
                _name = value;

                /*var bar = this.SearchComponent<HealthBarComponent>();
                 var bossBar = this.SearchComponent<BossHealthBarComponent>();
                 if (bar != null)
                     bar.Name = value;
                 if (bossBar != null)
                     bossBar.Name = value;*/
            }
        }

        public float Oxygen
        {
            get => _oxygen;
            set => _oxygen = Mathf.Clamp(value, 0, MaxOxygen);
        }

        public Vector3 Position
        {
            get => Model.Position;
            set => BlockPosition = value;
        }

        public Vector3 Rotation
        {
            get => Model.TargetRotation;
            set => Model.TargetRotation = value;
        }

        public string Type { get; set; } = MobType.None.ToString();
        
        public Entity()
        {
            _tickSystem = new TickSystem();
            ComponentManager = new EntityComponentManager(this);
            Physics = new PhysicsComponent(this);
            _splashTimer = new Timer(1f) { AutoReset = false };
        }

        public void ShowIcon(CacheItem? IconType)
        {
            this.ShowIcon(IconType, -1);
        }

        public void ShowIcon(CacheItem? IconType, float Seconds)
        {
            var iconComponent = this.SearchComponent<HeadIconComponent>();
            if (iconComponent == null)
            {
                iconComponent = new HeadIconComponent(this);
                this.AddComponent(iconComponent);
            }
            if(Seconds < 0)
                iconComponent.ShowIcon(IconType);
            else
                iconComponent.ShowIconFor(IconType, Seconds);
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound = true)
        {
            for (var i = 0; i < Components.Count; i++)
                if (Components[i] is DamageComponent)
                    _damageManager = Components[i] as DamageComponent;
            Exp = 0;
            if (_damageManager == null)
                return;

            Damager?.InvokeBeforeAttack(this, Amount);
            _damageManager.Damage(Amount, Damager, out Exp, PlaySound);
            Damager?.InvokeAfterAttack(this, Amount);

            if (Damager != null)
            {
                Vector3 increment = (-(Damager.Position.Xz - Position.Xz)).ToVector3();
                Physics.DeltaTranslate(increment * .2f);
            }
        }

        public void InvokeBeforeAttack(IEntity Invoker, float Damage)
        {
            BeforeAttacking?.Invoke(Invoker, Damage);
        }
        
        public void InvokeAfterAttack(IEntity Invoker, float Damage)
        {
            OnAttacking?.Invoke(Invoker, Damage);
        }

        public bool InAttackRange(IEntity Target, float RadiusModifier = 1f)
        {
            var collider0 = this.Model.BroadphaseCollider;
            var collider1 = Target.Model.BroadphaseCollider;
            var radii = (collider0.BroadphaseRadius + collider1.BroadphaseRadius) * RadiusModifier;
            if ((collider0.BroadphaseCenter - collider1.BroadphaseCenter).LengthSquared > radii * radii) return false;
            var vertices0 = collider0.Vertices;
            var vertices1 = collider1.Vertices;
            float lowestDistance = float.MaxValue;
            for (var i = 0; i < vertices0.Length; i++)
            {
                for (var k = 0; k < vertices1.Length; k++)
                {
                    var dist = (new Vector3(vertices0[i].X, this.Model.Position.Y, vertices0[i].Z) - new Vector3(vertices1[k].X, Target.Model.Position.Y, vertices1[k].Z)).LengthFast;
                    if (dist < lowestDistance)
                    {
                        lowestDistance = dist;
                    }
                }
            }
            return lowestDistance < 4f * RadiusModifier;
        }

        public void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition)
        {
            this.AddComponentWhile(new SpeedBonusComponent(this, BonusSpeed), Condition);
        }

        public void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds)
        {
            this.AddComponentForSeconds(new SpeedBonusComponent(this, BonusSpeed), Seconds);
        }

        public void AddComponentWhile(IComponent<IEntity> Component, Func<bool> Condition)
        {
            ComponentManager.AddComponentWhile(Component, Condition);
        }

        public void AddComponentForSeconds(IComponent<IEntity> Component, float Seconds)
        {
            ComponentManager.AddComponentForSeconds(Component, Seconds);
        }

        public void AddComponent(IComponent<IEntity> Component)
        {
            if(Component == null) throw new ArgumentNullException($"{this.GetType()} component cannot be null");
            Components.Add(Component);
            if(Component is ITickable tickable) _tickSystem.Add(tickable);
        }

        public void RemoveComponent(IComponent<IEntity> Component)
        {
            if (Component == null) throw new ArgumentNullException($"{this.GetType()} component cannot be null");
            Components.Remove(Component);
            Component.Dispose();
            if (Component is ITickable tickable) _tickSystem.Remove(tickable);
        }

        public T SearchComponent<T>() where T : IComponent<IEntity>
        {
            for (var i = 0; i < Components.Count; i++)
                if (Components[i] is T variable)
                    return variable;
            return default(T);
        }

        public void UpdateEnviroment()
        {
            var underChunk = World.GetChunkAt(Position);
            var waterHeight = PhysicsSystem.Physics.WaterHeight(Position);
            if (Position.Y + Model.Height < waterHeight && PhysicsSystem.Physics.WaterLevelAtPosition(Position) > Model.Height)
            {
                if (!Splashed && Math.Abs(waterHeight - Position.Y - Model.Height) > 4)
                {
                    SplashEffect(underChunk);
                    _splashTimer.Reset();
                    Splashed = true;
                }
                IsUnderwater = true;
            }
            else
            {
                IsUnderwater = false;
                Splashed = false;
            }
            this._splashTimer.Tick();
            this.HandleOxygen(waterHeight);
        }

        public bool IsUnderwater
        {
            get => _isUnderwater;
            private set
            {
                if (value == IsUnderwater) return;
                if (value)
                {
                    Physics.GravityDirection = IsHumanoid ? Vector3.Zero : Vector3.UnitY * .5f;
                    IsGrounded = false;
                    Physics.ResetFall();
                    Physics.ResetVelocity();
                }
                else
                {
                    Physics.GravityDirection = -Vector3.UnitY;
                }

                _isUnderwater = value;
            }
            
        }
        
        private void HandleOxygen(float WaterHeight)
        {
            // If the character is not moving and it's on the surface, then we shouldnt reduce the oxygen levels.
            if (IsUnderwater && (this.Position.Y + Model.Height + 1 < WaterHeight || IsMoving)) Oxygen -= Time.DeltaTime * 2f;
            else Oxygen += Time.DeltaTime * 4f;

            if (!(Oxygen <= 0) || !(Time.DeltaTime > 0)) return;
            Damage(Time.DeltaTime * 5f, null, out _, _drowningSoundTimer == 128);
            if (_drowningSoundTimer == 128)
                _drowningSoundTimer = 0;
            _drowningSoundTimer++;
        }
        
        public void SplashEffect(Chunk UnderChunk)
        {
            World.Particles.VariateUniformly = true;
            World.Particles.Color = new Vector4((UnderChunk?.Biome.Colors.WaterColor ?? Colors.DeepSkyBlue).Xyz, .5f);
            World.Particles.Position = Position;
            World.Particles.Scale = Vector3.One * .5f;
            World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            World.Particles.Direction = Vector3.UnitY;
            World.Particles.ParticleLifetime = 1;
            World.Particles.GravityEffect = .05f;
            World.Particles.PositionErrorMargin = new Vector3(3f, 3f, 3f);
            SoundManager.PlaySoundWithVariation(SoundType.WaterSplash, Position, 1f, .5f);
            for (var i = 0; i < 30; i++) World.Particles.Emit();
        }

        public void KnockForSeconds(float Time)
        {
            IsKnocked = true;
            Physics.Translate(Vector3.UnitY * 7.5f);
            _knockedTime = Time;
        }

        public void SpawnAnimation()
        {
            if (PlaySpawningAnimation)
            {
                PlaySpawningAnimation = false;
                if(Model.Enabled)
                    SoundManager.PlaySound(SoundType.GlassBreakInverted, BlockPosition, false, 1f, .8f);

                if (Model is IDisposeAnimation animable)
                {
                    animable.DisposeAnimation();
                    _spawningWithAnimation = true;
                    animable.DisposeTime = 1;
                    Model.Alpha = .1f;
                }
                else
                {
                    Model.Alpha = 1;
                }
            }
            if (_spawningWithAnimation)
            {
                if (Model is IDisposeAnimation animable)
                {
                    if (animable.DisposeTime < 0)
                    {
                        _spawningWithAnimation = false;
                        animable.Recompose();
                        Model.Alpha = 1;
                    }
                    Model.Alpha += Time.IndependantDeltaTime * 2.0f * .25f;
                    animable.DisposeTime -= Time.IndependantDeltaTime * 2.0f;
                }
            }
        }

        public virtual void Dispose()
        {
            if (Model.Disposed) return;

            World.RemoveEntity(this);

            for (var i = 0; i < Components.Count; i++)
                Components[i].Dispose();

            (Model as IAudible)?.StopSound();

            Physics.Dispose();
            Model.Dispose();
        }

        public virtual void Draw()
        {
            if (IsDead) return;

            Physics.Draw();
            for (var i = 0; i < Components.Count; i++)
                if (!Components[i].Renderable)
                    Components[i].Draw();
        }

        public virtual void Update()
        {
            this.Model.Update();

            if (IsDead) return;

            this.SpawnAnimation();
            PhysicsSystem.Physics.Threading.AddCommand(this);
            this.UpdateEnviroment();
            this._tickSystem.Tick();
            for (var i = 0; i < this.Components.Count; i++)
                this.Components[i].Update();
            if (IsKnocked)
            {
                _knockedTime -= Time.DeltaTime;
                if (Model is HumanoidModel model)
                {
                    model.Human.IsRiding = false;
                }
                if (_knockedTime < 0) IsKnocked = false;
            }
        }
    }
}