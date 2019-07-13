/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 08/12/2016
 * Time: 12:25 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Linq;
using Hedra.Components;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering.UI;
using Hedra.Sound;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.Player
{

    public delegate void OnHitLandedEventHandler(IHumanoid Humanoid, int ConsecutiveHits);
    
    public class Humanoid : Entity, IHumanoid
    {
        public const int MaxLevel = 99;
        public const int MaxConsecutiveHits = 45;
        public const float DefaultDodgeCost = 25;
        
        public event OnAttackEventHandler AfterAttack;
        public event OnAttackEventHandler BeforeAttack;
        public event OnHitLandedEventHandler OnHitLanded;
        private EquipmentHandler Equipment { get; }
        
        public ClassDesign Class
        {
            get => _class;
            set
            {
                _class = value;
                AttackResistance = _class.AttackResistance;
            }
        }

        public IPlayerInventory Inventory { get; }
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
        public bool IsSleeping { get; set; }
        public bool IsJumping => Movement.IsJumping;
        public float BaseAttackSpeed { get; private set; }
        public float ManaRegenFactor { get; set; }
        public float AttackPower { get; set; }
        public float BonusHealth { get; set; }
        public float DodgeCost { get; set; }    
        public float RandomFactor { get; set; }
        
        public virtual bool CanInteract { get; set; } = true;
        public virtual float FacingDirection => throw new NotImplementedException();
        public new HumanoidModel Model { get => base.Model as HumanoidModel; set => base.Model = value; }
        public virtual Vector3 LookingDirection => Orientation;
        public MovementManager Movement { get; protected set; }
        public HandLamp HandLamp { get; }
        private float _mana;
        private float _xp;
        private ClassDesign _class;
        private float _stamina = 100f;
        private bool _isGliding;
        private float _speedAddon;
        private readonly Timer _consecutiveHitsTimer;
        private DamageComponent _damageHandler;
        private bool _ignoreHitCombo;

        #region Propierties ( MaxMana, MaxHealth, MaxXp)

        public override float AttackingSpeedModifier => Class.AttackingSpeedModifier;
        
        public float BaseSpeed => Class.BaseSpeed;

        public override float MaxHealth => CalculateMaxHealth(Class, Level, RandomFactor) + BonusHealth;

        public static float CalculateMaxHealth(ClassDesign Class, int Level, float RandomFactor)
        {
            var maxHealth = 97 + RandomFactor * 20f;
            for (var i = 1; i < Level; i++)
            {
                maxHealth += Class.MaxHealthFormula(RandomFactor);
            }
            return maxHealth;
        }

        public float MaxXP => MaxLevel == Level ? 0 : ClassDesign.XPFormula(this.Level);
                    
        public float MaxMana
        {
            get
            {
                var maxMana = 90 + RandomFactor * 60f;
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
                var baseRegen = 4 + ManaRegenFactor;
                return baseRegen * (this.IsSleeping ? 6.0f : 1.0f);
            }
        }

        public float HealthRegen => this.IsSleeping ? 6.0f * this.MaxHealth * .005f : 0;

        public float StaminaRegen { get; set; } = 4;
        
        #endregion

        public Humanoid()
        {
            _consecutiveHitsTimer = new Timer(3f);
            Inventory = new DummyInventory();
            MessageDispatcher = new DummyMessageDispatcher();
            HandLamp = new HandLamp(this);
            Movement = new MovementManager(this);
            Equipment = new EquipmentHandler(this);
            Class = new WarriorDesign();
            RandomFactor = NewRandomFactor();
            Physics.CollidesWithStructures = true;
            DodgeCost = DefaultDodgeCost;
            AttackPower = 1f;
            Speed = this.BaseSpeed;
            BaseAttackSpeed = .75f;
            AddComponent(_damageHandler = new DamageComponent(this));
        }

        public override void Update()
        {
            base.Update();
            Movement.Update();
            Equipment.Update();
            UpdateHits();
            UpdateStats();
        }

        private void UpdateStats()
        {
            Stamina += Time.DeltaTime * StaminaRegen;
            if (!this.IsDead)
            {
                if(!_damageHandler.HasBeenAttacked)
                    Health += HealthRegen * Time.IndependentDeltaTime;
                Mana += ManaRegen * Time.IndependentDeltaTime;
            }
        }

        public void ResetEquipment()
        {
            Equipment.Reset();
        }

        private void UpdateHits()
        {
            if (!GameSettings.Paused && _consecutiveHitsTimer.Tick())
            {
                ConsecutiveHits = 0;
            }
        }
        
        #region Dodge
        public void Roll(RollType Type)
        {
            var player = this as LocalPlayer;
            if( IsUnderwater || IsTravelling || IsRolling || IsCasting || IsRiding || IsJumping) return;
            
            if(player != null)
            {
                if (Stamina < DodgeCost)
                {
                    GameManager.Player.MessageDispatcher.ShowNotification(Translations.Get("too_tired"), Color.DarkRed, 3f, true);
                    return;
                }
                Stamina -= DodgeCost;
            }
            IsAttacking = false;
            IsRolling = true;
            SearchComponent<DamageComponent>().Immune = true;
            WasAttacking = false;
            IsAttacking = false;
            Physics.ResetSpeed();
            this.ComponentManager.AddComponentWhile(new SpeedBonusComponent(this, -this.Speed + this.Speed * 1.1f),
                () => IsRolling);

            Movement.OrientateTowards(Movement.RollFacing);
            Movement.Move(Movement.LastOrientation * 2.5f * Attributes.TumbleDistanceModifier, .5f, false);

            SoundPlayer.PlaySoundWithVariation(SoundType.Dodge, this.Position);
            TaskScheduler.When( () => !IsRolling, () =>
            {
                SearchComponent<DamageComponent>().Immune = false;
            } );
        }

        #endregion

        public void AttackSurroundings(float Damage, IEntity[] Ignore)
        {
            this.AttackSurroundings(Damage, Ignore, null);
        }
        
        public void AttackSurroundings(float Damage, Action<IEntity> Callback)
        {
            this.AttackSurroundings(Damage, null, null);
        }
        
        public void AttackSurroundings(float Damage)
        {
            this.AttackSurroundings(Damage, null, null);
        }


        public void AttackSurroundings(float Damage, IEntity[] IgnoreList, Action<IEntity> Callback)
        {
            var meleeWeapon = LeftWeapon as MeleeWeapon;
            var rangeModifier =  meleeWeapon?.MainWeaponSize.Y / 2.5f + 1f ?? 1.0f;
            var wideModifier = Math.Max( (meleeWeapon?.MainWeaponSize.Xz.LengthFast ?? 1.0f) - .75f, 1.0f);
            var nearEntities = World.InRadius<IEntity>(this.Position, 32f * rangeModifier);
            var possibleTargets = nearEntities.Where(E => !E.IsStatic && E != this).ToArray();
            var atLeastOneHit = false;

            foreach (var target in possibleTargets)
            {
                if (IgnoreList != null && Array.IndexOf(IgnoreList, target) != -1) continue;
                var norm = (target.Position - this.Position).Xz.NormalizedFast().ToVector3();
                var dot = Vector3.Dot(norm, this.Orientation);
                if(dot > 0.60f / wideModifier && this.InAttackRange(target, rangeModifier))
                {
                    var damageToDeal = Damage * dot;
                    target.Damage(damageToDeal, this, out var exp, out var inflicted);
                    Callback?.Invoke(target);
                    this.XP += exp;
                    if (inflicted > 0)
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
            if(_ignoreHitCombo) return;
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

        public void DoIgnoringHitCombo(Action Lambda)
        {
            _ignoreHitCombo = true;
            try
            {
                Lambda();
            }
            finally
            {
                _ignoreHitCombo = false;
            }
        }

        public static float NewRandomFactor()
        {
            return Utils.Rng.NextFloat() * 1f + .0f;
        }

        public void SetWeapon(Weapon Item)
        {
            Equipment.SetWeapon(Item);
        }
        
        public void SetHelmet(HelmetPiece Item)
        {
            Equipment.SetHelmet(Item);
        }
        
        public void SetChestplate(ChestPiece Item)
        {
            Equipment.SetChest(Item);
        }
        
        public void SetPants(PantsPiece Item)
        {
            Equipment.SetPants(Item);
        }
        
        public void SetBoots(BootsPiece Item)
        {
            Equipment.SetBoots(Item);
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
            var defaultChance = 20;
            var defaultDamage = 50;
            var defaultDuration = 3;
            switch (NewType)
            {
                case EffectType.Fire:
                    effect = new FireComponent(this, defaultChance, defaultDamage, defaultDuration);
                    break;
                case EffectType.Poison:
                    effect = new PoisonousComponent(this, defaultChance, defaultDamage, defaultDuration);
                    break;
                case EffectType.Bleed:
                    effect = new BleedComponent(this, defaultChance, defaultDamage, defaultDuration);
                    break;
                case EffectType.Freeze:
                    effect = new FreezeComponent(this, defaultChance, defaultDamage, defaultDuration);
                    break;
                case EffectType.Speed:
                    effect = new SpeedComponent(this, defaultDuration);
                    break;
                case EffectType.Slow:
                    effect = new SlowComponent(this, defaultChance, defaultDamage, defaultDuration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NewType), NewType, null);
            }
            ComponentManager.AddComponentWhile(effect, Condition);
        }

        public void InvokeBeforeAttackEvent(AttackOptions Options)
        {
            BeforeAttack?.Invoke(Options);   
        }

        public void InvokeAfterAttackEvent(AttackOptions Options)
        {
            AfterAttack?.Invoke(Options);
        }

        public bool HasWeapon => MainWeapon != null;
        
        public float MaxStamina => Class.MaxStamina;

        public override float AttackResistance { get; set; }

        public float ConsecutiveHitsModifier => Mathf.Clamp(ConsecutiveHits / 90f, 0f, .5f);

        public float DamageEquation => (UnRandomizedDamageEquation * ( .75f + Utils.Rng.NextFloat() * .5f)) * Balancer.HumanoidDamageMultiplier;

        public float UnRandomizedDamageEquation => BaseDamageEquation * (1f + ConsecutiveHitsModifier);
        
        public float BaseDamageEquation => (Class.BaseDamage + Level * 0.08f) * AttackPower + this.WeaponModifier(MainWeapon);

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
            set => SetXP(value, false);
        }

        protected void SetXP(float Amount, bool Silent)
        {
            if (Level == MaxLevel) return;
            _xp = Amount;
            if (_xp < MaxXP) return;

            _xp = Amount - MaxXP;
            if (++Level == MaxLevel)
            {
                _xp = 0;
            }

            Health = MaxHealth;
            Mana = MaxMana;

            if (!Silent)
            {
                var label1 = new TextBillboard(4.0f, Translations.Get("level_up"), Color.Violet,
                    FontCache.GetBold(48),
                    () => this.Position)
                {
                    Scalar = .7f,
                    Vanish = true,
                };
                SoundPlayer.PlaySound(SoundType.NotificationSound, Position, false, 1, .65f);
            }

            /* So it keeps looping */
            if (_xp >= MaxXP) XP = _xp;
        }
        
        public virtual int Gold { get; set; }

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
        
        public Weapon LeftWeapon => Equipment.LeftWeapon;
        
        public virtual Item MainWeapon
        {
            get => Equipment.MainWeapon;
            set => Equipment.MainWeapon = value;
        }
        
        public virtual Item Ring
        {
            get => Equipment.Ring;
            set => Equipment.Ring = value;
        }

        public Item[] GetMainEquipment() => Equipment.MainEquipment;
        
        public Animation AnimationBlending => Model.AnimationBlending;
        
        public void ResetModel() => Model.Reset();

        public void BlendAnimation(Animation Animation) => Model.BlendAnimation(Animation);

        public bool CaptureMovement
        {
            set => Movement.CaptureMovement = value;
        }

        public void Orientate() => Movement.Orientate();

        public bool InAttackStance
        {
            set => LeftWeapon.InAttackStance = value;
        }
        
        public void SetMainEquipment(Item[] MainEquipment) => Equipment.MainEquipment = MainEquipment;

        public override void Dispose()
        {
            base.Dispose();
            Equipment.Dispose();
            this.HandLamp.Dispose();
        }

        public bool CanCastSkill => throw new NotImplementedException();
        
        public void SetSkillPoints(Type Skill, int Points) => throw new NotImplementedException();

        public T SearchSkill<T>() where T : AbstractBaseSkill => throw new NotImplementedException();
 
    }
}
