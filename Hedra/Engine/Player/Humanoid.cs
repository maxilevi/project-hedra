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

namespace Hedra.Engine.Player
{

	public class Humanoid : Entity
	{
        public virtual IMessageDispatcher MessageDispatcher { get; set; }
	    public int ConsecutiveHits { get; private set; }
        public bool IsAttacking {get; set;}
		public bool IsEating { get; set; }
		public bool IsCasting { get; set; }
		public bool IsSwimming { get; set; }
		public bool IsRolling { get; set; }
		public bool IsMoving { get; set; }
		public bool IsRiding { get; set; }
		public bool IsClimbing { get; set; }
		public bool WasAttacking { get; set; }
	    public float BaseAttackSpeed { get; private set; } = 1;
        public virtual bool CanInteract {get; set; }
        public bool IsSleeping { get; set; }
	    public bool IsJumping => Movement.IsJumping;
	    public virtual Vector3 FacingDirection => Vector3.UnitY * -( (float) Math.Acos(this.Orientation.X) * Mathf.Degree - 90f);
        public bool IsSitting { get => Model.IsSitting; set{ if(value) Model.Sit(); else Model.Idle(); } }
		public new HumanoidModel Model { get => base.Model as HumanoidModel; set => base.Model = value; }
		public MovementManager Movement { get; protected set; }
		public HandLamp HandLamp;
		public DamageComponent DmgComponent;
		public ClassDesign Class { get; set; } = new WarriorDesign();
	    public float AttackPower { get; set; }
		public float MaxStamina {get; set;}
		public float AddonHealth {get; set;}
		public float DodgeCost {get; set;}	
        public float RandomFactor { get; set; }
	    public virtual int Gold { get; set; }
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

	    public override float MaxHealth{
			get{
			    float maxHealth = 97 + RandomFactor * 20f;
			    for (var i = 1; i < this.Level; i++)
			    {
			        maxHealth += Class.MaxHealthFormula(RandomFactor);
			    }
			    return maxHealth + AddonHealth;			    
			}
		}

		public float MaxXP => this.MaxXpForLevel(this.Level);

	    protected float MaxXpForLevel(int TargetLevel)
	    {
	        return TargetLevel * 10f + 38;
	    }
        			
		public float MaxMana{
			get{
				float maxMana = 103 + RandomFactor * 34f;
				for(var i = 1; i < this.Level; i++){
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

	    public float HealthRegen
	    {
	        get
	        {
	            var baseRegen = this.MaxHealth * .005f;
	            return baseRegen * (this.IsSleeping ? 6.0f : 1.0f);
            }
	    }

	    #endregion

        public Humanoid() {
            this._consecutiveHitsTimer = new Timer(3f);

            this.CanInteract = true;
            this.MessageDispatcher = new DummyMessageDispatcher();
            this.HandLamp = new HandLamp(this);
			this.Movement = this.CreateMovementManager();
            this.DmgComponent = new DamageComponent(this);
            this.RandomFactor = Humanoid.NewRandomFactor();
            this.Physics.CanCollide = true;
            this.DodgeCost = 25f;
            this.MaxStamina = 100f;
            this.AttackPower = 1f;
            this.Speed = this.BaseSpeed;
            this.MobType = MobType.Human;
            this.AddComponent(DmgComponent);
        }

	    public override void Update()
	    {
	        base.Update();
            Movement.Update();
	        if (!GameSettings.Paused && _consecutiveHitsTimer.Tick())
	        {
	            ConsecutiveHits = 0;
	        }
        }

	    protected virtual MovementManager CreateMovementManager()
	    {
	        return new MovementManager(this);
	    }

        #region Dodge
        public void Roll(){
			var player = this as LocalPlayer;
			if( IsUnderwater || IsGliding || IsRolling || IsCasting || IsRiding || !IsGrounded) return;
			
			if(player != null){
			    if (Stamina < DodgeCost)
			    {
			        LocalPlayer.Instance.MessageDispatcher.ShowNotification("YOU ARE TOO TIRED.", Color.DarkRed, 3f, true);
			        return;
			    }
			    Stamina -= DodgeCost;		
			}
			Model.Model.Animator.StopBlend();
			IsAttacking = false;
            IsRolling = true;
            DmgComponent.Immune = true;
            this.ComponentManager.AddComponentWhile(new SpeedBonusComponent(this, -this.Speed + this.Speed * 1.5f),
                () => IsRolling);
            Movement.Move(this.Orientation * 2f, 1.5f, false);
            SoundManager.PlaySoundWithVariation(SoundType.Dodge, this.Position);
			Model.Roll();
			
		}
		
		public void FinishRoll(){
			IsRolling = false;
			DmgComponent.Immune = false;
		}
		#endregion
		
		public void Climb()
		{
			var frontBlock = World.GetBlockAt( this.BlockPosition + this.Orientation.Xz.ToVector3() * Chunk.BlockSize + Vector3.UnitY * 2f  );
			if(frontBlock.Type != BlockType.Air){
				Model.Run();
				this.Physics.UsePhysics = false;
				this.BlockPosition += Vector3.UnitY * 8 *(float) Time.deltaTime;
				this.Model.Position += Vector3.UnitY * 8 *(float) Time.deltaTime;
				this.Stamina -= (float) Time.deltaTime * 25f;
			}
		}
		
		public void EndClimb(){
			this.Physics.UsePhysics = true;
		}

        public void Attack(float Damage)
	    {
	        this.Attack(Damage, null);
	    }


	    public void Attack(float Damage, Action<Entity> Callback)
	    {
	        var rangeModifier = this.Model.LeftWeapon is MeleeWeapon meleeWeapon ? meleeWeapon.MainWeaponSize.Y / 3.75f + .5f : 1.0f;
            var nearEntities = World.InRadius<Entity>(this.Position, 16f * rangeModifier); // this.Model.BroadphaseCollider.BroadphaseRadius
            var possibleTargets = nearEntities.Where(E => !E.IsStatic && E != this).ToArray();
			var atLeastOneHit = false;
            foreach (var target in possibleTargets)
			{
				var dot = Vector3.Dot( (target.Position - this.Position).NormalizedFast(), this.Orientation);
				if(dot > 0.80f && this.InAttackRange(target, rangeModifier))
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
                if (!Class.CanAccumulateHits) return;
                _consecutiveHitsTimer.Reset();
	            ConsecutiveHits++;
	            int consecutiveHitsValue = ConsecutiveHits;
	            this.AddBonusAttackSpeedWhile(ConsecutiveHitsModifier, () => ConsecutiveHits == consecutiveHitsValue);
	            Mana = Mathf.Clamp(Mana + 8, 0, MaxMana);
            }
        }

	    protected static float NewRandomFactor()
	    {
	        return Utils.Rng.NextFloat() * .5f + .75f;
	    }

	    public void AddBonusAttackSpeedWhile(float BonusAttackSpeed, Func<bool> Condition)
	    {
	        ComponentManager.AddComponentWhile(new AttackSpeedBonusComponent(this, BonusAttackSpeed), Condition);
	    }
	    public void AddBonusHealthWhile(float BonusHealth, Func<bool> Condition)
	    {
	        ComponentManager.AddComponentWhile(new HealthBonusComponent(this, BonusHealth), Condition);
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

	    public float ConsecutiveHitsModifier => Mathf.Clamp(ConsecutiveHits / 75f, 0f, .75f);

	    public float DamageEquation => BaseDamageEquation * (.75f + Utils.Rng.NextFloat() + Utils.Rng.NextFloat() * .6f) * (1f + ConsecutiveHitsModifier);

	    public float BaseDamageEquation => (this.Level * 2.75f + 16f) * this.WeaponModifier(MainWeapon) * this.AttackPower;

	    public float WeaponModifier(Item Weapon)
	    {
	        if (Weapon == null) return 0.2f;
            var tierModifier = 1.0f + (int)Weapon.Tier / ((int)ItemTier.Divine + 1.0f);
	        return  Weapon.GetAttribute<float>(CommonAttributes.Damage) * tierModifier / 15.0f;
        }

	    public float AttackSpeed{
			get{
				float attackSpeed = BaseAttackSpeed;
				if(MainWeapon != null) attackSpeed *= MainWeapon.GetAttribute<float>(CommonAttributes.AttackSpeed);
				return attackSpeed;
			}
            set => this.BaseAttackSpeed = value;
	    }
		
        public float XP {
			get => _xp;
            set{
				_xp = value;
			    if (!(_xp >= MaxXP)) return;
			    _xp -= MaxXP;
			    Level++;
					
			    Health = MaxHealth;
			    Mana = MaxMana;

			    var label = new Billboard(4.0f, "LEVEL UP!", Color.Violet,
			        FontCache.Get(AssetManager.BoldFamily, 48, FontStyle.Bold),
			        this.Model.Position)
			    {
			        Size = .7f,
			        Vanish = true,
			        FollowFunc = () => this.Position
			    };

			    SoundManager.PlaySound(SoundType.NotificationSound, Position, false, 1, .65f);
			    //make a loop
			    if(_xp >= MaxXP)
			        XP = _xp;
			}
		}
		
        public virtual Item Ring { 
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

	
        public float Stamina {
			get => _stamina;
            set => _stamina = Mathf.Clamp(value,0,MaxStamina);
        }
		

		public float Mana{
			get => _mana;
		    set => _mana = Mathf.Clamp(value,0,MaxMana);
		}
		
		public bool IsGliding
		{
			get => _isGliding;
		    set{ 
				if(value){
					if(IsGrounded)
						return;
				}
				_isGliding = value;
			}
		}

	    public override void Dispose()
	    {
            base.Dispose();
	        this.HandLamp.Dispose();
        }
	}
}
