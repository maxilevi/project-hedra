/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:51 a.m.
 *
 */

using System;
using System.Collections.Generic;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Sound;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>S
    ///     Description of Class1.
    /// </summary>
    public delegate void OnDamagingEventHandler(IEntity Victim, float Amount);

    public delegate void OnAttackEventHandler(AttackOptions Options);

    public delegate void OnDamageModifierEventHandler(IEntity Victim, ref float Damage);

    public delegate void OnComponentAdded(IComponent<IEntity> Component);

    public delegate void OnKillEventHandler(DeadEventArgs Args);
    
    public class Entity : IEntity
    {
        private readonly object _componentsLock = new object();
        public virtual float AttackingSpeedModifier => 0.75f;
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
        private BaseUpdatableModel _model;
        public IPhysicsComponent Physics { get; }

        private readonly List<IComponent<IEntity>> _components = new List<IComponent<IEntity>>();
        private bool Splashed { get; set; }

        public event OnComponentAdded ComponentAdded;
        public event OnDamagingEventHandler AfterDamaging;
        public event OnDamagingEventHandler BeforeDamaging;
        public event OnDamageModifierEventHandler DamageModifiers;
        public event OnKillEventHandler Kill;
        public event OnDisposedEvent AfterDisposed;
        public EntityAttributes Attributes { get; }
        public EntityComponentManager ComponentManager { get; }
        public float AttackDamage { get; set; } = 1.0f;
        public float AttackCooldown { get; set; }
        public virtual float AttackResistance { get; set; } = 1.0f;
        public int Level { get; set; } = 1;
        public float MaxOxygen { get; set; } = 30;
        public int MobId { get; set; }
        public int Seed { get; set; }
        public Vector3 Orientation { get; set; } = Vector3.UnitZ;       
        public bool Removable { get; set; } = true;
        public bool IsStuck => Physics.IsStuck;
        public bool PlaySpawningAnimation { get; set; } = true;
        public bool IsAttacking => Model.IsAttacking;
        public float Speed { get; set; } = 1.25f;
        public bool IsMoving => Model.IsMoving;
        public bool IsUndead => Model.IsUndead;

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

        public bool InUpdateRange => (Physics.RigidbodyPosition - LocalPlayer.Instance.Model.Position).Xz.LengthSquared < GeneralSettings.UpdateDistanceSquared;

        public bool IsActive { get; set; }
        public bool IsBoss { get; set; }
        public bool IsDead { get; set; }
        public virtual bool IsFriendly { get; set; }
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

        public Vector3 Size => Model.Dimensions.Size;

        public BaseUpdatableModel Model
        {
            get => _model;
            set
            {
                _model = value;
                Physics.SetHitbox(_model.Dimensions);
            }
        }

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
            set => _name = value;
        }

        public float Oxygen
        {
            get => _oxygen;
            set => _oxygen = Mathf.Clamp(value, 0, MaxOxygen);
        }

        public Vector3 Position
        {
            get => Physics.RigidbodyPosition;
            set => Physics.Translate(-Position + value);
        }

        public Vector3 Rotation
        {
            get => Model.TargetRotation;
            set => Model.TargetRotation = value;
        }

        public string Type { get; set; } = MobType.None.ToString();

        protected Entity()
        {
            _tickSystem = new TickSystem();
            Attributes = new EntityAttributes();
            ComponentManager = new EntityComponentManager(this);
            Physics = new PhysicsComponent(this);
            _splashTimer = new Timer(1f) { AutoReset = false };
            BeforeDamaging += OnBeforeDamaging;
            AfterDamaging += OnAfterDamaging;
        }
        
        private void OnBeforeDamaging(IEntity Victim, float Damage)
        {
            Victim.SearchComponent<DamageComponent>().OnDeadEvent += OnDead;
        }
        
        private void OnAfterDamaging(IEntity Victim, float Damage)
        {
            Victim.SearchComponent<DamageComponent>().OnDeadEvent -= OnDead;
        }

        private void OnDead(DeadEventArgs Args)
        {
            Kill?.Invoke(Args);
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

        public void Damage(float Amount, IEntity Damager, out float Exp, bool PlaySound = true, bool PushBack = true)
        {
            Damage(Amount, Damager, out Exp, out _, PlaySound, PushBack);
        }

        public void Damage(float Amount, IEntity Damager, out float Exp, out float Inflicted, bool PlaySound = true, bool PushBack = true)
        {
            for (var i = 0; i < _components.Count; i++)
            {
                if (_components[i] is DamageComponent dmg)
                    _damageManager = dmg;
            }

            Exp = 0;
            Inflicted = 0;
            if (_damageManager == null)
                return;

            Damager?.InvokeBeforeDamaging(this, Amount);
            Damager?.InvokeDamageModifier(this, ref Amount);
            _damageManager.Damage(Amount, Damager, out Exp, out Inflicted, PlaySound, PushBack);
            Damager?.InvokeAfterDamaging(this, Amount);
        }

        public void InvokeBeforeDamaging(IEntity Invoker, float Damage)
        {
            BeforeDamaging?.Invoke(Invoker, Damage);
        }
        
        public void InvokeAfterDamaging(IEntity Invoker, float Damage)
        {
            AfterDamaging?.Invoke(Invoker, Damage);
        }

        public void InvokeDamageModifier(IEntity Invoker, ref float Damage)
        {
            DamageModifiers?.Invoke(Invoker, ref Damage);
        }

        public void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition, bool ShowParticles = true)
        {
            AddComponentWhile(new SpeedBonusComponent(this, BonusSpeed, ShowParticles), Condition);
        }

        public void AddBonusSpeedForSeconds(float BonusSpeed, float Seconds)
        {
            AddComponentForSeconds(new SpeedBonusComponent(this, BonusSpeed), Seconds);
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
            ComponentAdded?.Invoke(Component);
            lock(_componentsLock)
                _components.Add(Component);
            if(Component is ITickable tickable) _tickSystem.Add(tickable);
        }

        public void RemoveComponent(IComponent<IEntity> Component, bool Dispose = true)
        {
            if (Component == null) throw new ArgumentNullException($"{this.GetType()} component cannot be null");
            lock(_componentsLock)
                _components.Remove(Component);
            if (Component is ITickable tickable) _tickSystem.Remove(tickable);
            if (Dispose)Component.Dispose();
        }

        public T SearchComponent<T>()
        {
            for (var i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T variable)
                    return variable;
            }

            return default(T);
        }
        
        public T[] GetComponents<T>()
        {
            var list = new List<T>();
            for (var i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T variable)
                    list.Add(variable);
            }
            return list.ToArray();
        }

        private void UpdateEnvironment()
        {
            if (!(this is LocalPlayer)) return;
            var underChunk = World.GetChunkAt(Position);
            var waterHeight = PhysicsSystem.Physics.WaterHeight(Position);
            if (Position.Y + Model.Height < waterHeight /* && PhysicsSystem.Physics.WaterLevelAtPosition(Position) > Model.Height*/)
            {
                if (!Splashed && Math.Abs(waterHeight - Position.Y - Model.Height) > 4)
                {
                    SplashEffect(underChunk);
                    _splashTimer.Reset();
                    Splashed = true;
                }
                IsUnderwater = true;
            }
            else if(Position.Y + Model.Height > waterHeight + Chunk.BlockSize)
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

        private void SplashEffect(Chunk UnderChunk)
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
            SoundPlayer.PlaySoundWithVariation(SoundType.WaterSplash, Position, 1f, .5f);
            for (var i = 0; i < 30; i++) World.Particles.Emit();
        }

        public void KnockForSeconds(float Seconds)
        {
            IsKnocked = true;
            var accum = 0f;
            TaskScheduler.While(
                () => accum < 7.5f,
                () =>
                {
                    var curr = 40f * Time.DeltaTime;
                    accum += curr;
                    Physics.Translate(Vector3.UnitY * curr);
                }
            );
            _knockedTime = Seconds;
        }

        public void SpawnAnimation()
        {
            if (PlaySpawningAnimation)
            {
                PlaySpawningAnimation = false;
                if(Model.Enabled)
                    SoundPlayer.PlaySound(SoundType.GlassBreakInverted, Position, false, 1f, .8f);

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
                    Model.Alpha += Time.IndependentDeltaTime * 2.0f * .25f;
                    animable.DisposeTime -= Time.IndependentDeltaTime * 2.0f;
                }
            }
        }

        public virtual void Dispose()
        {
            if (Disposed) return;

            World.RemoveEntity(this);
            var components = _components.ToArray(); 
            for (var i = components.Length-1; i > -1; --i)
            {
                components[i]?.Dispose();
            }

            (Model as IAudible)?.StopSound();

            Physics.Dispose();
            Model.Dispose();
            AfterDisposed?.Invoke();
        }

        public virtual void Draw()
        {
            if (IsDead) return;

            Physics.Draw();
            for (var i = _components.Count - 1; i > -1; --i)
            {
                if (!(_components[i] is IRenderable))
                    _components[i].Draw();
            }
        }

        public virtual void Update()
        {
            this.Model.Update();
            this.Physics.Update();

            if (IsDead) return;

            this.SpawnAnimation();
            this.UpdateEnvironment();
            this._tickSystem.Tick();
            var beforeComponents = default(IComponent<IEntity>[]);
            lock (_componentsLock)
                beforeComponents = _components.ToArray();
            for (var i = beforeComponents.Length - 1; i > -1; --i)
            {
                beforeComponents[i]?.Update();
            }

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

        public bool Disposed => Model.Disposed;
    }
}