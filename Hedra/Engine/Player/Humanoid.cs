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
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player
{

	public class Humanoid : Entity
	{
        public virtual IMessageDispatcher MessageDispatcher { get; set; }
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
		public bool IsSitting { get{ return Model.IsSitting; } set{ if(value) Model.Sit(); else Model.Idle(); } }
		public new HumanModel Model { get{ return base.Model as HumanModel; } set{ base.Model = value;} }
		public MovementManager Movement;
		public HandLamp HandLamp;
		public DamageComponent DmgComponent;
		public virtual Item MainWeapon { get; set; }
		public ClassDesign Class { get; set; } = new WarriorDesign();
	    public float AttackPower { get; set; }
		public float MaxStamina {get; set;}
		public float AddonHealth {get; set;}
		public float DodgeCost {get; set;}	
        public float RandomFactor { get; set; }
	    public virtual int Gold { get; set; }
	    private Item _ring;
	    private float _mana;
	    private float _xp;
	    private float _stamina = 100f;
	    private bool _isGliding;
	    private float _speedAddon;

        #region Propierties ( MaxMana, MaxHealth, MaxXp)

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
		public float MaxXP{
			get{
				var maxXp = 38;
				for(var i = 1; i < Level; i++)
					maxXp = (int) (maxXp * 1.15f);
				return maxXp;
			}
		}				
		public float MaxMana{
			get{
				float maxMana = 103 + RandomFactor * 34f;
				for(var i = 1; i < this.Level; i++){
                    maxMana += Class.MaxHealthFormula(RandomFactor);				    
                }
				return maxMana;
			}
		}

	    public float ManaRegen
	    {
	        get
	        {
	            var baseRegen = 2.75f;
	            for (var i = 1; i < Level; i++)
	                baseRegen = baseRegen * 1.15f;
	            return baseRegen * (this.IsSleeping ? 6.0f : 1.0f);
            }
	    }

	    public float HealthRegen
	    {
	        get
	        {
	            var baseRegen = .75f;
	            for (var i = 1; i < Level; i++)
	                baseRegen = baseRegen * 1.15f;
	            return baseRegen * (this.IsSleeping ? 6.0f : 1.0f);
            }
	    }

	    #endregion

        public Humanoid() : base() {
			this.CanInteract = true;
            this.MessageDispatcher = new DummyMessageDispatcher();
            this.HandLamp = new HandLamp(this);
			this.Movement = new MovementManager(this);
            this.DmgComponent = new DamageComponent(this);
            this.RandomFactor = Humanoid.NewRandomFactor();
            this.DefaultBox.Max = new Vector3(4f, 4, 4f);
            this.Physics.CanCollide = true;
            this.DodgeCost = 25f;
            this.MaxStamina = 100f;
            this.AttackPower = 1f;
            this.Speed = this.BaseSpeed;
            this.AddComponent(DmgComponent);
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
            Movement.OrientateWhileMoving = false;	
			if(!IsMoving){
				Movement.MoveCount = 2;
				Movement.MoveFeet = true;
			}
            SoundManager.PlaySoundWithVariation(SoundType.Dodge, this.Position);
			Model.Roll();
			
		}
		
		public void FinishRoll(){
			IsRolling = false;
			DmgComponent.Immune = false;
			Movement.OrientateWhileMoving = true;
			Movement.MoveFeet = false;
		}
		#endregion
		
		public void Climb(){
			Block frontBlock = World.GetBlockAt( this.BlockPosition + this.Orientation.Xz.ToVector3() * Chunk.BlockSize + Vector3.UnitY * 2f  );
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

	    public bool AttackEntity(float AttackDamage, Entity Mob)
	    {
	        return this.AttackEntity(AttackDamage, Mob, null);
	    }

        public bool AttackEntity(float AttackDamage, Entity Mob, Action<Entity> Callback)
		{
		    if (!this.InAttackRange(Mob)) return false;

		    Callback?.Invoke(Mob);


		    float Exp;
		    Mob.Damage( AttackDamage, this, out Exp);
		    this.XP += Exp;
		    return true;
		}

	    public void Attack(float AttackDamage)
	    {
	        this.Attack(AttackDamage, null);
	    }


	    public void Attack(float AttackDamage, Action<Entity> Injection){
			
			float highestDot = 0;
			Entity dotEntity = null;
	        var hittedSomething = false;
			for(int i = World.Entities.Count-1; i > -1; i--)
			{
			    if (World.Entities[i].IsFriendly) continue;
			    if (World.Entities[i] == this) continue;

			    if (World.Entities[i] != null)
			    {
			        if (this.AttackEntity(AttackDamage, World.Entities[i], Injection))
			            hittedSomething = true;
			    }
			}
            if(!hittedSomething) MainWeapon?.Weapon.PlaySound();
			Mana = Mathf.Clamp(Mana + 8, 0 , MaxMana);
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

	    public float DamageEquation => BaseDamageEquation * (.75f + Utils.Rng.NextFloat() + Utils.Rng.NextFloat() * .6f);

	    public float BaseDamageEquation{
			get
			{
			    float dmgToDo = this.Level * 16.0f + 4.0f;
                dmgToDo *= this.WeaponModifier(MainWeapon);
                return dmgToDo * this.AttackPower;
			}
		}

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
            set
            {
                this.BaseAttackSpeed = value;
            }
		}
		
        public float XP {
			get{ return _xp; }
			set{
				_xp = value;
			    if (!(_xp >= MaxXP)) return;
			    _xp -= MaxXP;
			    Level++;
					
			    Health = MaxHealth;
			    Mana = MaxMana;

			    var label = new Billboard(4.0f, "LEVEL UP!", Color.Violet,
			        FontCache.Get(AssetManager.Fonts.Families[0], 48, FontStyle.Bold),
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
			get{ return _ring; }
			set{
			    if (this.Ring == value)  return;             
			    _ring = value;

			    if (this.Ring != null)
			    {
			        var effectType = (EffectType) Enum.Parse(typeof(EffectType), _ring.GetAttribute<string>("EffectType"));
                    if (effectType != EffectType.None) this.ApplyEffectWhile(effectType, () => this.Ring == value);

			        this.AddBonusSpeedWhile(this.Ring.GetAttribute<float>("MovementSpeed"), () => this.Ring == value);
			        this.AddBonusAttackSpeedWhile( this.Ring.GetAttribute<float>("AttackSpeed"), () => this.Ring == value);
			        this.AddBonusHealthWhile(this.MaxHealth * (this.Ring.GetAttribute<float>("Health")*0.1f), () => this.Ring == value);
                }
			}
		}

	
        public float Stamina {
			get{ return _stamina; }
			set{ _stamina = Mathf.Clamp(value,0,MaxStamina); }
		}
		

		public float Mana{
			get{ return _mana; }
			set{ _mana = Mathf.Clamp(value,0,MaxMana); }
		}
		
		public bool IsGliding
		{
			get{ return _isGliding; }
			set{ 
				if(value){
					if(IsGrounded)
						return;
					this.Movement.UnlockAnimations = true;
				}else{
					this.Movement.UnlockAnimations = false;
				}
				_isGliding = value;
			}
		}
	}
}
