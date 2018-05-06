/*
 * Author: Zaphyk
 * Date: 26/02/2016
 * Time: 04:51 a.m.
 *
 */

using System;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    ///     Description of Class1.
    /// </summary>
    public delegate void OnAttackEventHandler(Entity Victim, float Amount);

    public class Entity : IUpdatable, IRenderable, IDisposable, ISearchable//, ICullable
    {
        private DamageComponent _damageManager;
        private int _drowningSoundTimer;
        private float _health = 100f;
        private int _isHumanoid = -1;
        private bool _knocked;
        private string _name;
        private float _oxygen = 30;
        private float _prevHeight;
        private float _previousFalltime;
        private bool _spawningWithAnimation;
        private float _knockedTime;
        private readonly TickSystem _tickSystem;

        public EntityComponentManager ComponentManager { get; }
        public float AttackDamage { get; set; } = 1;
        protected List<EntityComponent> Components = new List<EntityComponent>();
        public Box BaseBox { get; private set; } = new Box(Vector3.Zero, Vector3.One);
        public bool Destroy { get; set; } = false;
        public int Level { get; set; } = 1;
        public float MaxOxygen { get; set; } = 30;
        public int MobId { get; set; }
        public int MobSeed { get; set; }
        public Vector3 Orientation { get; set; } = Vector3.UnitZ;
        public PhysicsComponent Physics { get; set; }
        public bool Removable { get; set; } = true;
        protected bool Splashed { get; set; }
        public Vector3 BlockPosition { get; set; }
        public bool PlaySpawningAnimation { get; set; } = true;
        public float Speed { get; set; } = 2;

        public virtual float Health
        {
            get { return _health; }
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

        public Box HitBox => BaseBox.Cache.Translate(Model.Position);

        public bool InUpdateRange => (BlockPosition.Xz - LocalPlayer.Instance.Model.Model.Position.Xz).LengthSquared <
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

        public bool IsStatic => Model is StaticModel;
        public bool IsUnderwater { get; set; }

        public bool Knocked
        {
            get { return _knocked; }
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
                MobType enumeration;
                bool result = Enum.TryParse(Type, true, out enumeration);

                return result ? enumeration : MobType.Unknown;
            }

            set
            {
                if (value != MobType.Unknown)
                    Type = value.ToString();
            }
        }

        public Model Model { get; set; }

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
            get { return _oxygen; }
            set { _oxygen = Mathf.Clamp(value, 0, MaxOxygen); }
        }

        public Vector3 Position
        {
            get { return Model.Position; }
            set { BlockPosition = value; }
        }

        public Vector3 Rotation
        {
            get { return Model.TargetRotation; }
            set { Model.TargetRotation = value; }
        }

        public RenderShape Shape { get; set; }
        public Vector3 Size { get; set; }

        public string Type { get; set; } = MobType.None.ToString();
        public bool WasGrounded { get; private set; }

        public Entity()
        {
            _tickSystem = new TickSystem();
            ComponentManager = new EntityComponentManager(this);
            Physics = new PhysicsComponent(this);
        }

        public void SetHitbox(Box NewBox)
        {
            this.BaseBox = NewBox;
        }

        public void MultiplyHitbox(Vector3 Scale)
        {
            this.BaseBox = BaseBox * Scale;
        }

        public void ShowIcon(CacheItem? IconType)
        {
            var iconComponent = this.SearchComponent<HeadIconComponent>();
            if (iconComponent == null)
            {
                iconComponent = new HeadIconComponent(this);
                this.AddComponent(iconComponent);
            }
            iconComponent.ShowIcon(IconType);
        }

        public event OnAttackEventHandler OnAttacking, BeforeAttacking;

        public void Damage(float Amount, Entity Damager, out float Exp, bool PlaySound = true)
        {
            for (var i = 0; i < Components.Count; i++)
                if (Components[i] is DamageComponent)
                    _damageManager = Components[i] as DamageComponent;
            Exp = 0;
            if (_damageManager == null)
                return;

            Damager?.BeforeAttacking?.Invoke(this, Amount);
            _damageManager.Damage(Amount, Damager, out Exp, PlaySound);
            Damager?.OnAttacking?.Invoke(this, Amount);

            if (Damager != null)
            {
                Vector3 increment = (-(Damager.Position.Xz - Position.Xz)).ToVector3();
                Physics.Move(increment * .2f * (float)Time.deltaTime);
            }
        }

        public bool InAttackRange(Entity Target, out float Dot)
        {
            Dot = Math.Max(Vector2.Dot(
                (Target.Position - this.Position).Xz.NormalizedFast(),
                this.Orientation.Xz.NormalizedFast()
                ), 0);

            return (Target.Position - Position).LengthFast < (BaseBox.Max - BaseBox.Min).LengthFast +
                   (Target.BaseBox.Max - Target.BaseBox.Min).LengthFast &&
                   Target != this &&
                   Dot > 0.7f;
        }

        public bool InAttackRange(Entity Target)
        {
            return this.InAttackRange(Target, out _);
        }

        public void AddBonusSpeedWhile(float BonusSpeed, Func<bool> Condition)
        {
            this.AddComponentWhile(new SpeedBonusComponent(this, BonusSpeed), Condition);
        }


        public void AddComponentWhile(EntityComponent Component, Func<bool> Condition)
        {
            ComponentManager.AddComponentWhile(Component, Condition);
        }

        public void AddComponent(EntityComponent Component)
        {
            if(Component == null) throw new ArgumentNullException($"{this.GetType()} component cannot be null");
            Components.Add(Component);
            var tickable = Component as ITickable;
            if(tickable != null) _tickSystem.Add(tickable);
        }

        public void RemoveComponent(EntityComponent Component)
        {
            if (Component == null) throw new ArgumentNullException($"{this.GetType()} component cannot be null");
            Components.Remove(Component);
            Component.Dispose();
            var tickable = Component as ITickable;
            if (tickable != null) _tickSystem.Remove(tickable);
        }

        public T SearchComponent<T>() where T : EntityComponent
        {
            for (var i = 0; i < Components.Count; i++)
                if (typeof(T).IsAssignableFrom(Components[i].GetType()))
                    return (T) Components[i];
            return default(T);
        }

        public void UpdateEnviroment()
        {
            if (!IsUnderwater)
                if (IsGrounded && !WasGrounded && _previousFalltime > 0.25f)
                    SoundManager.PlaySound(SoundType.HitGround, Position);
            if (this is LocalPlayer)
            {
                var a = 0;
            }

            float nearestWaterBlockY = float.MinValue;
            Chunk underChunk = World.GetChunkAt(Position);
            Vector3 blockSpace = World.ToBlockSpace(Position);
            if (underChunk == null) return;
            for (int y = underChunk.BoundsY - 1; y > -1; y--)
            {
                Block block = underChunk.GetBlockAt((int) blockSpace.X, y, (int) blockSpace.Z);
                if (block.Type == BlockType.Water)
                {
                    nearestWaterBlockY = y * Chunk.BlockSize;
                    break;
                }
            }

            if (nearestWaterBlockY > Position.Y + BaseBox.Max.Y - BaseBox.Min.Y + Chunk.BlockSize)
            {
                if (!Splashed)
                {
                    World.Particles.VariateUniformly = true;
                    World.Particles.Color = underChunk.Biome.Colors.WaterColor;
                    World.Particles.Position = Position;
                    World.Particles.Scale = Vector3.One * .5f;
                    World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                    World.Particles.Direction = Vector3.UnitY;
                    World.Particles.ParticleLifetime = 1;
                    World.Particles.GravityEffect = .05f;
                    World.Particles.PositionErrorMargin = new Vector3(3f, 3f, 3f);

                    if(!GameManager.InStartMenu)
                        SoundManager.PlaySoundWithVariation(SoundType.WaterSplash, Position);
                    for (var i = 0; i < 30; i++) World.Particles.Emit();
                    Splashed = true;
                }
                if (IsHumanoid)
                    Physics.ResetFall();

                if (!IsHumanoid)
                {
                    Physics.GravityDirection = Vector3.UnitY * .4f;
                    Physics.TargetPosition += Vector3.UnitY * .5f;
                    IsGrounded = false;
                    Physics.ResetFall();
                }
                else
                {
                    IsGrounded = false;
                    Physics.GravityDirection = Vector3.Zero;
                }
                IsUnderwater = true;
            }
            else if (Math.Abs(nearestWaterBlockY -
                              (Position.Y + BaseBox.Max.Y - BaseBox.Min.Y + Chunk.BlockSize)) >
                     Chunk.BlockSize * .5f)
            {
                if (IsUnderwater)
                    Physics.GravityDirection = -Vector3.UnitY;
                Splashed = false;
                IsUnderwater = false;
            }

            if (IsUnderwater) Oxygen -= (float) Time.deltaTime * 2f;
            else Oxygen += (float) Time.deltaTime * 4f;

            if (Oxygen <= 0 && Time.deltaTime > 0)
            {
                float xp;
                this.Damage((float) Time.deltaTime * 5f, null, out xp, _drowningSoundTimer == 128);
                if (_drowningSoundTimer == 128)
                    _drowningSoundTimer = 0;
                _drowningSoundTimer++;
            }

            WasGrounded = IsGrounded;
            _previousFalltime = Physics.Falltime;
        }

        public void KnockForSeconds(float Time)
        {
            Knocked = true;
            _knockedTime = Time;
        }

        public void SpawnAnimation()
        {
            if (PlaySpawningAnimation)
            {
                PlaySpawningAnimation = false;
                if(Model.Enabled)
                    SoundManager.PlaySound(SoundType.GlassBreakInverted, BlockPosition, false, 1f, .8f);

                var animable = Model as IDisposeAnimation;
                if (animable != null)
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
                var animable = Model as IDisposeAnimation;
                if (animable != null)
                {
                    if (animable.DisposeTime < 0)
                    {
                        _spawningWithAnimation = false;
                        animable.Recompose();
                        Model.Alpha = 1;
                    }
                    Model.Alpha += Time.FrameTimeSeconds * 2.0f * .25f;
                    animable.DisposeTime -= Time.FrameTimeSeconds * 2.0f;
                }
            }
        }

        public void Dispose()
        {
            if (Model.Disposed) return;

            World.RemoveEntity(this);

            for (var i = 0; i < Components.Count; i++)
                Components[i].Dispose();

            var humanoid = this as Humanoid;
            humanoid?.HandLamp.Dispose();

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
            PhysicsSystem.Physics.Manager.AddCommand(this);
            this.UpdateEnviroment();
            this._tickSystem.Tick();
            for (var i = 0; i < this.Components.Count; i++)
                this.Components[i].Update();
            this.Model.Rotation = Mathf.Lerp(Model.Rotation, Model.TargetRotation, (float) Time.deltaTime * 16f);
            if (Knocked)
            {
                _knockedTime -= Time.ScaledFrameTimeSeconds;
                var model = Model as HumanModel;
                if (model != null)
                {
                    model.KnockOut();
                    model.Human.IsRiding = false;
                }
                else
                {
                    Model.Idle();
                }
                if (_knockedTime < 0) Knocked = false;
            }
        }
    }
}