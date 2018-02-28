/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 23/04/2016
 * Time: 02:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Sound;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.Player
{
    /// <inheritdoc/>
    /// <summary>
    /// Description of PlayerModel.
    /// </summary>
    public class HumanModel : Model, IAudible, IDisposeAnimation
    {
		public const float DefaultScale = 0.75f;
		public Humanoid Human;
		public AnimatedModel Model;
		private Animation WalkAnimation;
		private Animation IdleAnimation;
		private Animation RollAnimation;
		private Animation EatAnimation;
		private Animation SwimAnimation, IdleSwimAnimation;
		private Animation RideAnimation, IdleRideAnimation;
		private Animation ClimbAnimation;
		private Animation SitAnimation;
		private Animation GlideAnimation;
		private Animation KnockedAnimation;
        private Animation TiedAnimation;
        private Animation SleepAnimation;   
        public Joint LeftHand;
        public Joint RightHand;
        public Joint Chest;
        public Joint LeftFoot;
        public Joint RightFoot;
        public Joint Head;
        private AreaSound _modelSound;

        public float FacingDirection;
		public StaticModel Food;
		public Weapon LeftWeapon { get; private set; }
		public QuadrupedModel MountModel;
		
		public override Vector3 TargetRotation {get; set;}
		public bool LockWeapon {get; set;}
	    public override Vector4 Tint { get; set; }
	    public override Vector4 BaseTint { get; set; }
	    public bool IsSitting => this.SitAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsRunning => this.WalkAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsGliding => this.GlideAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsSwimming => this.IdleSwimAnimation == this.Model.Animator.AnimationPlaying || this.SwimAnimation == this.Model.Animator.AnimationPlaying;

		public Vector4 Color0;
        public Vector4 Color1;
        private EntityMesh _lampModel;
	    private bool _hasLamp;
        private float _foodHealth;
        private Vector3 _previousPosition;
        private float _lastAnimationTime = -1;


        public HumanModel(Humanoid Human, HumanoidModelTemplate Template)
		{
		    this.Init(Human, Template);
		}

        public HumanModel(Humanoid Human, HumanType Type)
        {
            this.Init(Human, HumanoidLoader.ModelTemplater[Type]);
        }

        public HumanModel(Humanoid Human)
        {
            this.Init(Human, HumanoidLoader.ModelTemplater[Human.ClassType]);
        }

        public void Init(Humanoid Human, HumanoidModelTemplate Template){
			this.Human = Human;
			this.Scale = Vector3.One * Template.Scale;
			this.LeftWeapon = Weapon.Empty;
			this.Tint = Vector4.One;
			
			Model = AnimationModelLoader.LoadEntity(Template.Path);

            this.Model.Scale = DefaultScale * new Vector3(1, 1.15f, 1) * Template.Scale;// * 1.15f;
			this.LeftHand = Model.RootJoint.GetChild("Hand_L");
			this.RightHand = Model.RootJoint.GetChild("Hand_R");
			this.Chest = Model.RootJoint.GetChild("Chest");
			this.LeftFoot = Model.RootJoint.GetChild("Foot_L");
			this.RightFoot = Model.RootJoint.GetChild("Foot_R");
            this.Head = Model.RootJoint.GetChild("Head");
			
			WalkAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWalk.dae");
			IdleAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
			RollAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorRoll.dae");
			EatAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorEat.dae");
			SwimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwim.dae");
			IdleSwimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwimIdle.dae");
			ClimbAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorClimb.dae");
			RideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMount.dae");
			IdleRideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMountIdle.dae");
			SitAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSit.dae");
			GlideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorGlide.dae");
			KnockedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorKnocked.dae");
            TiedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTied.dae");
            SleepAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSleep.dae");
            WalkAnimation.Speed = 1.35f;

            RollAnimation.Loop = false;
			RollAnimation.OnAnimationEnd += delegate(Animation Sender) { 
				Human.Physics.ResetFall();
				Human.FinishRoll();
				this.Model.PlayAnimation(IdleAnimation);
			};

			EatAnimation.Loop = false;
            EatAnimation.OnAnimationStart += delegate
            {
                SoundManager.PlaySound(SoundType.FoodEat, this.Position);
            };

			EatAnimation.OnAnimationEnd += delegate{ 
				this.Food.Enabled = false;
				Human.Health += this._foodHealth * .5f;
				
				Human.IsEating = false;
				this.Model.Animator.ExitBlend();
			};

            this.Human.DefaultBox = AssetManager.LoadHitbox(Template.Path) * this.Model.Scale;
            this.Height = 1.65f;//this.Human.DefaultBox.Max.Y - this.Human.DefaultBox.Min.Y;

            this.Idle();
            this.Model.Size = this.Human.DefaultBox.Max - this.Human.DefaultBox.Min; 
            this._modelSound = new AreaSound(SoundType.HumanRun, Vector3.Zero, 48f);
        }

        public void Resize(Vector3 Scalar)
        {
            this.Model.Scale *= Scalar;
            this.Human.HitBox *= Scalar;
        }
		
		public bool DisableWeapon { 
			get{
				var player = Human as LocalPlayer;				
				return (player.Skills.W1 as WeaponAttack).DisableWeapon && (player.Skills.W2 as WeaponAttack).DisableWeapon; 
			}
			set{
				var Player = Human as LocalPlayer;
				(Player.Skills.W1 as WeaponAttack).DisableWeapon = value;
				(Player.Skills.W2 as WeaponAttack).DisableWeapon = value;
				LeftWeapon.Mesh.Enabled = !value;				
			}
		}
		
		public void UpdateModel(){}
		
		public void SetLamp(bool Active){
			if(this._hasLamp == Active) return;
			this._hasLamp = Active;
			
			if(this._lampModel == null){
				VertexData LampData = AssetManager.PlyLoader("Assets/Items/Handlamp.ply", new Vector3(1.5f, 1.5f, 1.5f), Vector3.Zero, Vector3.Zero, true);
	        	this._lampModel = EntityMesh.FromVertexData(LampData);
			}
			this._lampModel.Enabled = Active;
			
			base.Init(true);
		}

        public void DisposeAnimation()
        {
            Model.SwitchShader(AnimatedModel.DeathShader);
            DisposeTime = 0;
        }

        public float DisposeTime
        {
            get { return Model.DisposeTime; }
            set { Model.DisposeTime = value; }
        }

        public void Recompose()
        {
            Model.SwitchShader(AnimatedModel.DefaultShader);
            DisposeTime = 0;
        }

        public override void Attack(Entity Target, float Damage)
		{
			if(!Human.IsAttacking && !(Human is LocalPlayer)){
				//Human.AttackEntity(Human.DamageEquation, Target);
				LeftWeapon.Attack1(this);//Damage is the attack multiplier
			}
		}
		
		public void SetWeapon(Weapon Weapon){
			Init();
			if(Weapon == this.LeftWeapon)
				return;
			int Index = -1;
			for(int i = 0; i < base.Meshes.Length; i++){
				if(base.Meshes[i] == this.LeftWeapon.Mesh){
					Index = i;
					break;
				}
			}
			
			this.LeftWeapon.Enabled = false;
			this.LeftWeapon = Weapon;
			this.LeftWeapon.Enabled = true;
			base.Meshes[Index] = this.LeftWeapon.Mesh;
			
			this.LeftWeapon.Scale = Model.Scale;
			this.LeftWeapon.Alpha = Model.Alpha;
			
			if(Human is LocalPlayer){
				LocalPlayer Player = Human as LocalPlayer;
				if(Weapon is Bow){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.SHOOT);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.TRIPLESHOT);
				}
				
				if(Weapon is Knife){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.KNIFE_SLASH);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.KNIFE_LUNGE);
				}
				
				if(Weapon is ThrowingKnife){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.THROW);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.THROW_SPECIAL);
				}
				
				if(Weapon is Blade){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.BLADE1);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.BLADE2);
				}
				
				if(Weapon is Katar){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.KATAR1);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.KATAR2);
				}
				
				if(Weapon is Claw){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.CLAW1);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.CLAW2);
				}
				
				if(Weapon is Axe || Weapon is Hammer){
					(Player.Skills.W1 as WeaponAttack)?.SetType(WeaponAttack.AttackType.SWING);
					(Player.Skills.W2 as WeaponAttack)?.SetType(WeaponAttack.AttackType.SMASH);
				}
			}
		}
		
		public override void Run(){
		    Human.IsMoving = true;

            if (Human.IsRolling || Human.IsRiding  || Human.IsUnderwater || Human.IsAttacking || Human.IsGliding)
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != WalkAnimation)
				Model.PlayAnimation(WalkAnimation);
        }
		
		public void Glide(){

			if(Model != null && Model.Animator.AnimationPlaying != GlideAnimation)
				Model.PlayAnimation(GlideAnimation);
			
		}
		
		public void KnockOut(){

			if(Model != null && Model.Animator.AnimationPlaying != KnockedAnimation)
				Model.PlayAnimation(KnockedAnimation);
			
		}
		
		public override void Idle(){
		    Human.IsMoving = false;

            if (Human.IsCasting || Human.IsRiding || Human.IsGliding || Human.IsClimbing || Human.IsAttacking || Human.IsEating || Human.IsRiding || Human.IsRolling || Human.IsUnderwater)
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != IdleAnimation)
				Model.PlayAnimation(IdleAnimation);
        }		
		
		public void Climb(){
			if(Human.IsRolling || Human.IsUnderwater || Human.IsAttacking )
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != ClimbAnimation)
				Model.PlayAnimation(ClimbAnimation);
		}
		
		public void Sit(){
			if(Human.IsRolling || Human.IsUnderwater || Human.IsAttacking)
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != SitAnimation)
				Model.PlayAnimation(SitAnimation);
		}

        public void Sleep()
        {
            if (Model != null && Model.Animator.AnimationPlaying != SleepAnimation)
                Model.PlayAnimation(SleepAnimation);
        }

        public void Ride(){
			if(Human.IsMoving){
				if(Model != null && Model.Animator.AnimationPlaying != RideAnimation)
					Model.PlayAnimation(RideAnimation);
			}else{
				if(Model != null && Model.Animator.AnimationPlaying != IdleRideAnimation)
					Model.PlayAnimation(IdleRideAnimation);
			}
		}
		
		public void Eat(float FoodHealth){
			this._foodHealth = FoodHealth;
			this.Human.Health += FoodHealth * .5f;
			Model.Animator.StopBlend();
			Model.Animator.BlendAnimation(EatAnimation);
			this.Human.WasAttacking = false;
			this.Human.IsAttacking = false;
		}
		
		public void Swim(){
			if( !(Human is LocalPlayer) || Human.IsRolling || Human.IsCasting) return;
			if(Model != null && Model.Animator.AnimationPlaying != SwimAnimation)
				Model.PlayAnimation(SwimAnimation);
		}
		
		public void IdleSwim(){
			if( !(Human is LocalPlayer) || Human.IsRolling || Human.IsCasting) return;
			if(Model != null && Model.Animator.AnimationPlaying != IdleSwimAnimation)
				Model.PlayAnimation(IdleSwimAnimation);
		}
		
		public void Roll(){
			Model.Animator.PlayAnimation(RollAnimation);
			this.Human.WasAttacking = false;
			this.Human.IsAttacking = false;
		}
		
		public void Tied(){
		    if (Model != null && Model.Animator.AnimationPlaying != TiedAnimation)
                Model.PlayAnimation(TiedAnimation);
		}

        public void TiedSitting()
        {
            if (Model != null && Model.Animator.AnimationPlaying != SitAnimation)
                Model.PlayAnimation(SitAnimation);

            if (Model != null && Model.Animator.BlendingAnimation != TiedAnimation)
                Model.BlendAnimation(TiedAnimation);
        }

        public override void Update()
		{

		    if (this.Model.Animator.AnimationPlaying != this.RollAnimation && Human.IsRolling)
		        Human.FinishRoll();

            if (_lastAnimationTime != this.Model.Animator.AnimationTime || Scenes.SceneManager.Game.InMenu){
				_lastAnimationTime = this.Model.Animator.AnimationTime;
			}else{
				Model.Animator.AnimationPlaying.DispatchEvents(1f);
				Model.Animator.StopBlend();
			}
			
			if(Model.Animator.AnimationPlaying == null)
				this.Idle();
			
			if(Model != null){
				Model.Update();
				Vector3 PositionAddon = -Vector3.UnitY * 1.5f;
				if(this.MountModel != null)
					PositionAddon += (MountModel.Parent.HitBox.Max.Y - MountModel.Parent.HitBox.Min.Y) * Vector3.UnitY * 0.5f;
				
				Model.Position = this.Position + PositionAddon;
				Model.Rotation = Mathf.Lerp(Model.Rotation, this.TargetRotation, (float) Time.unScaledDeltaTime * 8f);
				this.Rotation = Model.Rotation;
				//this._shadow.Position = Model.Position;
				if(MountModel != null)
					this.MountModel.TargetRotation = this.TargetRotation;
			}
		
			if(Human.IsEating && this.Model.Animator.BlendingAnimation != EatAnimation){
				Human.IsEating = false;
				this.Food.Enabled = false;
			}else if(Human.IsEating){
				Matrix4 Mat4 = this.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + ((this.LeftHandPosition + this.RightHandPosition) / 2f) );
				
				this.Food.Mesh.TransformationMatrix = Mat4;
				this.Food.Mesh.Position = Model.Position;
				this.Food.Mesh.TargetPosition = Vector3.Zero;
				this.Food.Mesh.AnimationPosition = Vector3.Zero;
				this.Food.Mesh.TargetRotation = new Vector3(180,0,0);
				this.Food.Mesh.RotationPoint = Vector3.Zero;
				this.Food.Mesh.Rotation = Vector3.Zero;
				this.Food.Mesh.LocalRotation = Vector3.Zero;
				this.Food.Mesh.LocalPosition = Vector3.Zero;
				this.Food.Mesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				
				if(!this.Human.IsMoving && Model.Animator.AnimationPlaying == WalkAnimation){
					this.Model.Animator.ExitBlend();
				}
			}
			
			if(!LockWeapon)
				this.LeftWeapon.Update(this);
			
			if(this._hasLamp){
				this._lampModel.Position = this.LeftHandPosition;
				this._lampModel.Rotation = this.Rotation;
				this._lampModel.RotationPoint = Vector3.Zero;
			}
			Human.HandLamp.Update();
			if(Human.IsRiding)
				Ride();
			
			if(Human.IsSitting)
				Sit();

		    if (Human.IsSleeping)
		        Sleep();

			if(Human.IsRolling){
				if(_previousPosition != this.Human.BlockPosition && this.Human.IsGrounded){
					Block block = World.GetHighestBlockAt( (int) this.Human.Position.X, (int) this.Human.Position.Z);
				    World.WorldParticles.VariateUniformly = true;
				    World.WorldParticles.Color = Vector4.One;//World.GetHighestBlockAt( (int) this.Human.Position.X, (int) this.Human.Position.Z).GetColor(Region.Default);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.WorldParticles.Position = this.Human.Position - Vector3.UnitY;
				    World.WorldParticles.Scale = Vector3.One * .5f;
				    World.WorldParticles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.WorldParticles.Direction = (-this.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.WorldParticles.ParticleLifetime = 1;
				    World.WorldParticles.GravityEffect = .1f;
				    World.WorldParticles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					for(int i = 0; i < 1; i++){
					    World.WorldParticles.Emit();
					}
				}
				_previousPosition = this.Human.BlockPosition;
			}
			
			Model.BaseTint = Mathf.Lerp(Model.BaseTint, this.BaseTint, (float) Time.unScaledDeltaTime * 6f);
			Model.Tint = Mathf.Lerp(Model.Tint, this.Tint, (float) Time.unScaledDeltaTime * 6f);
			this.LeftWeapon.Mesh.Tint = Model.Tint;
			this.LeftWeapon.Mesh.BaseTint = Model.BaseTint;
			
			Model.Alpha = Mathf.Lerp(Model.Alpha, _targetAlpha, (float) Time.ScaledFrameTimeSeconds * 8f);
			this.LeftWeapon.Alpha =  Mathf.Lerp(LeftWeapon.Alpha, _targetAlpha, (float) Time.ScaledFrameTimeSeconds * 8f);
            //this._shadow.Enabled = Model.Alpha > 0.25f && this.Human.IsGrounded && this.Enabled;

		    if (!this.Disposed)
		    {
		        _modelSound.Type = Human.IsSleeping ? SoundType.HumanSleep : SoundType.HumanRun;
		        _modelSound.Position = this.Position;
		        _modelSound.Update(this.IsRunning || Human.IsSleeping);
		    }
		}

	    public void StopSound()
	    {
	        _modelSound.Stop();
	    }

        public void Draw(){
			if(!Enabled)
				return;
			this.Model.Draw();
			if(this.LeftWeapon.Meshes != null){
				for(int i = 0; i < this.LeftWeapon.Meshes.Length; i++){
					this.LeftWeapon.Meshes[i].Draw();
				}
			}
		}
		
		
		private float _targetAlpha = 1f;
		public override float Alpha {
			get { return _targetAlpha; }
			set {
				_targetAlpha = value;
					
			}
		}

        public Matrix4 ChestMatrix => Model.MatrixFromJoint(Chest);

        public Matrix4 LeftHandMatrix => Model.MatrixFromJoint(LeftHand);

	    public Matrix4 RightHandMatrix => Model.MatrixFromJoint(RightHand);

        private Vector3 _defaultHeadPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 HeadPosition
        {
            get
            {
                if (this._defaultHeadPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    this._defaultHeadPosition = Model.JointDefaultPosition(Head);
                return Model.TransformFromJoint(this._defaultHeadPosition, Head);
            }
        }

        private Vector3 _defaultLeftHandPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 LeftHandPosition{
			get{
				if(this._defaultLeftHandPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultLeftHandPosition = Model.JointDefaultPosition(LeftHand);
				return Model.TransformFromJoint(this._defaultLeftHandPosition, LeftHand);
			}
		}
		
		private Vector3 _defaultRightFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 RightFootPosition{
			get{
				if(this._defaultRightFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultRightFootPosition = Model.JointDefaultPosition(RightFoot);
				return Model.TransformFromJoint(this._defaultRightFootPosition, RightFoot);
			}
		}
		
		private Vector3 _defaultLeftFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 LeftFootPosition{
			get{
				if(this._defaultLeftFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultLeftFootPosition = Model.JointDefaultPosition(LeftFoot);
				return Model.TransformFromJoint(this._defaultLeftFootPosition, LeftFoot);
			}
		}
		
		private Vector3 _defaultRightHandPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 RightHandPosition{
			get{
				if(this._defaultRightHandPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultRightHandPosition = Model.JointDefaultPosition(RightHand);
				return Model.TransformFromJoint(this._defaultRightHandPosition, RightHand);
			}
		}

        private Vector3 _defaultChestPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 ChestPosition
        {
            get
            {
                if (this._defaultChestPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    this._defaultChestPosition = Model.JointDefaultPosition(Chest);
                return Model.TransformFromJoint(this._defaultChestPosition, Chest);
            }
        }

	    public override bool Fog{
			get{ return Model.Fog; }
			set{ Model.Fog = value;}
		}
		
		public override bool Pause {
			get { return base.Pause; }
			set { 
				base.Pause = value;
				Model.Animator.Stop = value;
			}
		}
		
		private bool _enabled = true;
		public override bool Enabled{
			get{ return _enabled; }
			set{
				this.LeftWeapon.Enabled = value;
				Model.Enabled = value;
				_enabled = value;
			}
		}
		
		private Vector3 _position;
		public override Vector3 Position{
			get{
                return _position;
                
			}
			set{
				if(MountModel != null)
					this.MountModel.Position = value - Vector3.UnitY * 1.5f;

                _position = value;
			}
		}

	    public override Vector3 Rotation { get; set; }

	    public override void Dispose()
		{
			Model.Dispose();
			//DrawManager.DropShadows.Remove(this._shadow);
			LeftWeapon.Dispose();
			
			/*BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (FieldInfo Field in this.GetType().GetFields(Flags))
			{
				if(Field.GetType() == typeof(Animation))
					(Field.GetValue(this) as Animation).Dispose();
			}*/
			base.Dispose();
		}
	}
	
	public enum HumanType{
		Warrior,
		Archer,
		Rogue,
		Skeleton,
		Merchant,
        Blacksmith,
		Mandragora,
        TravellingMerchant,
        Villager
	}
}
