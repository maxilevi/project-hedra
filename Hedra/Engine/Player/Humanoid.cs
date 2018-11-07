/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 12:25 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Sound;
using System.Drawing;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.UI;
using System.Linq;
using Hedra.Engine.Game;

namespace Hedra.Engine.Player
{

    public delegate void OnHitLandedEventHandler(IHumanoid Humanoid, int ConsecutiveHits);
    
    public class Humanoid : Entity, IHumanoid
    {
        public const int MaxLevel = 99;
        public const int MaxConsecutiveHits = 45;
        public const float DefaultDodgeCost = 25;
        public event OnHitLandedEventHandler OnHitLanded;
        public IMessageDispatcher MessageDispatcher { get; set; }
        public int ConsecutiveHits { get; private set; }
        public bool IsAttacking { get; set; }
        public bool IsEating { get; set; }
        public bool IsCasting { get; set; }
        public bool IsSwimming { get; set; }
        public bool IsRolling { get; set; }
        public bool IsRiding { get; set; }
        public bool IsTied { get; set; }
        public bool IsClimbing { get; set; }
        public bool WasAttacking { get; set; }
        public bool IsSitting { get; set; }
        public float BaseAttackSpeed { get; private set; } = .75f;
        public virtual bool CanInteract { get; set; } = true;
        public bool IsSleeping { get; set; }
        public bool IsJumping => Movement.IsJumping;
        public virtual float FacingDirection => -( (float) Math.Acos(this.Orientation.X) * Mathf.Degree - 90f);
        public new HumanoidModel Model { get => base.Model as HumanoidModel; set => base.Model = value; }
        public MovementManager Movement { get; protected set; }
        public HandLamp HandLamp { get; }
        public FishingHandler Fisher { get; }
        public DamageComponent DmgComponent;
        public ClassDesign Class { get; set; } = new WarriorDesign();
        public float AttackPower { get; set; }
        public float AddonHealth { get; set; }
        public float DodgeCost { get; set; }    
        public float RandomFactor { get; set; }
        public virtual int Gold { get; set; }
        public Weapon LeftWeapon => Model.LeftWeapon;
        private Item _mainWeapon;
        private Item _ring;
        private float _mana;
        private float _xp;
        private float _stamina = 100f;
        private bool _isGliding;
        private float _speedAddon;
        private readonly Timer _consecutiveHitsTimer;

        #region Propierties ( MaxMana, MaxHealth, MaxXp)

        public virtual Item MainWeapon
        {
            get => _mainWeapon;
            set
            {
                if(_mainWeapon == value) return;
                _mainWeapon = value;
                Model.SetWeapon(_mainWeapon?.Weapon ?? Weapon.Empty);           
            }
        }

        public float BaseSpeed => Class.BaseSpeed;

        public override float MaxHealth
        {
            get
            {
                var maxHealth = 97 + RandomFactor * 20f;
                for (var i = 1; i < this.Level; i++)
                {
                    maxHealth += Class.MaxHealthFormula(RandomFactor);
                }
                return maxHealth + AddonHealth;                
            }
        }

        public float MaxXP => MaxLevel == Level ? 0 : Class.XPFormula(this.Level);
                    
        public float MaxMana
        {
            get
            {
                var maxMana = 180 + RandomFactor * 60f;
                for(var i = 1; i < this.Level; i++)
                {
                    maxMana += Class.MaxManaFormula(RandomFactor);                    
                }
                return maxMana;
            }
        }

        public float ManaRegen
        {
            get
            {
                var baseRegen = this.MaxMana * .01f;
                return baseRegen * (this.IsSleeping ? 6.0f : 1.0f);
            }
        }

        public float HealthRegen => this.IsSleeping ? 6.0f * this.MaxHealth * .005f : 0;

        #endregion

        public Humanoid()
        {
            _consecutiveHitsTimer = new Timer(3f);
            MessageDispatcher = new DummyMessageDispatcher();
            HandLamp = new HandLamp(this);
            Movement = new MovementManager(this);
            DmgComponent = new DamageComponent(this);
            Fisher = new FishingHandler(this);
            RandomFactor = NewRandomFactor();
            Physics.CanCollide = true;
            DodgeCost = DefaultDodgeCost;
            AttackPower = 1f;
            Speed = this.BaseSpeed;
            MobType = MobType.Human;
            AddComponent(DmgComponent);
        }

        public override void Update()
        {
            base.Update();
            Movement.Update();
            if (!GameSettings.Paused && _consecutiveHitsTimer.Tick())
            {
                ConsecutiveHits = 0;
            }
            Fisher.Update();
        }

        #region Dodge
        public void Roll()
        {
            var player = this as LocalPlayer;
            if( IsUnderwater || IsTravelling || IsRolling || IsCasting || IsRiding || IsJumping) return;
            
            if(player != null){
                if (Stamina < DodgeCost)
                {
                    LocalPlayer.Instance.MessageDispatcher.ShowNotification("YOU ARE TOO TIRED.", Color.DarkRed, 3f, true);
                    return;
                }
                Stamina -= DodgeCost;        
            }
            IsAttacking = false;
            IsRolling = true;
            DmgComponent.Immune = true;
            WasAttacking = false;
            IsAttacking = false;
            this.ComponentManager.AddComponentWhile(new SpeedBonusComponent(this, -this.Speed + this.Speed * 1.1f),
                () => IsRolling);
            Movement.Move(this.Orientation * 2f, 1f, false);
            SoundManager.PlaySoundWithVariation(SoundType.Dodge, this.Position);
            TaskManager.When( () => !IsRolling, () =>
            {
                DmgComponent.Immune = false;
            } );
        }

        #endregion

        public void Attack(float Damage)
        {
            this.Attack(Damage, null);
        }


        public void Attack(float Damage, Action<Entity> Callback)
        {
            var meleeWeapon = this.Model.LeftWeapon as MeleeWeapon;
            var rangeModifier =  meleeWeapon?.MainWeaponSize.Y / 3.75f + .5f ?? 1.0f;
            var wideModifier = Math.Max( (meleeWeapon?.MainWeaponSize.Xz.LengthFast ?? 1.0f) - .75f, 1.0f);
            var nearEntities = World.InRadius<Entity>(this.Position, 16f * rangeModifier); // this.Model.BroadphaseCollider.BroadphaseRadius
            var possibleTargets = nearEntities.Where(E => !E.IsStatic && E != this).ToArray();
            var atLeastOneHit = false;

            foreach (var target in possibleTargets)
            {
                var norm = (target.Position - this.Position).Xz.NormalizedFast().ToVector3();
                var dot = Vector3.Dot(norm, this.Orientation);
                if(dot > 0.80f / wideModifier && this.InAttackRange(target, rangeModifier))
                {
                    var damageToDeal = Damage * dot;
                    target.Damage(damageToDeal, this, out float exp);
                    Callback?.Invoke(target);
                    this.XP += exp;
                    atLeastOneHit = true;
                }
            }

            if(!atLeastOneHit)
            {
                MainWeapon?.Weapon.PlaySound();
            }
            this.ProcessHit(atLeastOneHit);
        }

        public void ProcessHit(bool HittedSomething)
        {
            if (!HittedSomething)
            {
                ConsecutiveHits = 0;
            }
            else
            {
                _consecutiveHitsTimer.Reset();
                var previousValue = ConsecutiveHits;
                ConsecutiveHits = Math.Min(MaxConsecutiveHits, ++ConsecutiveHits);
                var consecutiveHitsValue = ConsecutiveHits;
                if (previousValue != MaxConsecutiveHits)
                {
                    this.AddBonusAttackSpeedWhile(ConsecutiveHitsModifier * .5f, () => ConsecutiveHits == consecutiveHitsValue);
                }
                Mana = Mathf.Clamp(Mana + 8, 0, MaxMana);
                OnHitLanded?.Invoke(this, ConsecutiveHits);
            }
        }

        public static float NewRandomFactor()
        {
            return Utils.Rng.NextFloat() * 1f + .0f;
        }

        public void AddBonusAttackSpeedWhile(float BonusAttackSpeed, Func<bool> Condition)
        {
            ComponentManager.AddComponentWhile(new AttackSpeedBonusComponent(this, BonusAttackSpeed), Condition);
        }
        public void AddBonusHealthWhile(float BonusHealth, Func<bool> Condition)
        {
            ComponentManager.AddComponentWhile(new HealthBonusComponent(this, BonusHealth), Condition);
        }

        public void Greet()
        {
            CanInteract = false;
            Movement.CaptureMovement = false;
            Model.Greet(delegate
            {
                CanInteract = true;
                Movement.CaptureMovement = true;
            });
        }

        public void ApplyEffectWhile(EffectType NewType, Func<bool> Condition)
        {
            EntityComponent effect;
            switch (NewType)
            {
                case EffectType.Fire:
                    effect = new FireComponent(this);
                    break;
                case EffectType.Poison:
                    effect = new PoisonousComponent(this);
                    break;
                case EffectType.Bleed:
                    effect = new BleedComponent(this);
                    break;
                case EffectType.Freeze:
                    effect = new FreezeComponent(this);
                    break;
                case EffectType.Speed:
                    effect = new SpeedComponent(this);
                    break;
                case EffectType.Slow:
                    effect = new SlowComponent(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NewType), NewType, null);
            }
            ComponentManager.AddComponentWhile(effect, Condition);
        }

        public bool HasWeapon => MainWeapon != null;
        
        public float MaxStamina => Class.MaxStamina;
        
        public override float AttackResistance => (1 - 0.003f * base.Level) / Class.AttackResistance;
        
        public float ConsecutiveHitsModifier => Mathf.Clamp(ConsecutiveHits / 35f, 0f, 1.25f);

        public float DamageEquation => UnRandomizedDamageEquation * ( .75f + Utils.Rng.NextFloat() * .5f);

        public float UnRandomizedDamageEquation => BaseDamageEquation * (1f + ConsecutiveHitsModifier);
        
        public float BaseDamageEquation => (Class.BaseDamage + this.Level * 0.08f * this.AttackPower) + this.WeaponModifier(MainWeapon);

        public float WeaponModifier(Item Weapon)
        {
            return Weapon?.GetAttribute<float>(CommonAttributes.Damage) ?? 0.0f;
        }

        public float AttackSpeed
        {
            get
            {
                var attackSpeed = BaseAttackSpeed;
                if(MainWeapon != null) attackSpeed *= MainWeapon.GetAttribute<float>(CommonAttributes.AttackSpeed);
                return attackSpeed;
            }
            set => this.BaseAttackSpeed = value;
        }
        
        public float XP
        {
            get => _xp;
            set
            {
                if(Level == MaxLevel) return;
                _xp = value;
                if (_xp < MaxXP) return;
                _xp = value - MaxXP;
                if (++Level == MaxLevel)
                {
                    _xp = 0;
                }
                
                Health = MaxHealth;
                Mana = MaxMana;

                var label1 = new Billboard(4.0f, "LEVEL UP!", Color.Violet,
                    FontCache.Get(AssetManager.BoldFamily, 48, FontStyle.Bold),
                    this.Position)
                {
                    Size = .7f,
                    Vanish = true,
                    FollowFunc = () => this.Position
                };
                SoundManager.PlaySound(SoundType.NotificationSound, Position, false, 1, .65f);
                
                /* So it keeps looping */
                if(_xp >= MaxXP) XP = _xp;
            }
        }
        
        public virtual Item Ring
        { 
            get => _ring;
            set{
                if (this.Ring == value)  return;             
                _ring = value;

                if (this.Ring != null)
                {
                    var effectType = (EffectType) Enum.Parse(typeof(EffectType), _ring.GetAttribute<string>("EffectType"));
                    if (effectType != EffectType.None) this.ApplyEffectWhile(effectType, () => this.Ring == value);

                    this.AddBonusSpeedWhile(this.Ring.GetAttribute<float>("MovementSpeed"), () => this.Ring == value);
                    this.AddBonusAttackSpeedWhile(this.AttackSpeed * this.Ring.GetAttribute<float>("AttackSpeed"), () => this.Ring == value);
                    this.AddBonusHealthWhile(this.MaxHealth * this.Ring.GetAttribute<float>("Health"), () => this.Ring == value);
                }
            }
        }

    
        public float Stamina
        {
            get => _stamina;
            set => _stamina = Mathf.Clamp(value, 0, MaxStamina);
        }
        

        public float Mana
        {
            get => _mana;
            set => _mana = Mathf.Clamp(value,0,MaxMana);
        }
        
        public bool IsFishing { get; set; }
        
        public virtual bool IsSailing => false;

        public virtual bool IsGliding => false;

        public virtual bool IsTravelling
        {
            get => false;
            set => throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.HandLamp.Dispose();
        }
    }
}
