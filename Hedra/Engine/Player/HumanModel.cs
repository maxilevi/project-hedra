/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 23/04/2016
 * Time: 02:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using Hedra.Engine.ComplexMath;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.Generation;
using Hedra.Engine.Sound;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
    /// <inheritdoc/>
    /// <summary>
    /// Description of PlayerModel.
    /// </summary>
    public class HumanModel : EntityModel, IAudible, IDisposeAnimation
    {
		public const float DefaultScale = 0.75f;
		public Humanoid Human { get; private set; }
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
        public Joint LeftWeaponJoint;
        public Joint RightWeaponJoint;
        public Joint ChestJoint;
        public Joint LeftFootJoint;
        public Joint RightFootJoint;
        public Joint HeadJoint;
        private AreaSound _modelSound;
		public StaticModel Food;
		public Weapon LeftWeapon { get; private set; }
		public QuadrupedModel MountModel;
		
		public override Vector3 TargetRotation {get; set;}
		public bool LockWeapon {get; set;}
	    public override Vector4 Tint { get; set; }
	    public override Vector4 BaseTint { get; set; }
        public bool IsIdling => this.IdleAnimation == this.Model.Animator.AnimationPlaying;
        public bool IsSitting => this.SitAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsRunning => this.WalkAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsGliding => this.GlideAnimation == this.Model.Animator.AnimationPlaying;
	    public bool IsSwimming => this.IdleSwimAnimation == this.Model.Animator.AnimationPlaying || this.SwimAnimation == this.Model.Animator.AnimationPlaying;
        private string _modelPath;
        private ObjectMesh _lampModel;
	    private bool _hasLamp;
        private float _foodHealth;
        private Vector3 _previousPosition;
        private Quaternion _rotationQuaternion;
        private float _lastAnimationTime = -1;
        private float _targetAlpha = 1f;


        public HumanModel(Humanoid Human, HumanoidModelTemplate Template) : base(Human)
		{
		    this.Load(Human, Template);
		}

        public HumanModel(Humanoid Human, HumanType Type) : base(Human)
        {
            this.Load(Human, HumanoidLoader.ModelTemplater[Type]);
        }

        public HumanModel(Humanoid Human) : base(Human)
        {
            this.Load(Human, HumanoidLoader.ModelTemplater[Human.Class]);
        }

        public void Paint(Vector4[] Colors)
        {
            if(Colors.Length > AssetManager.ColorCodes.Length)
                throw new ArgumentOutOfRangeException("Provided amount of colors cannot be higher than the color codes.");

            var colorMap = new Dictionary<Vector3, Vector3>();
            for (var i = 0; i < Colors.Length; i++)
            {
                colorMap.Add(AssetManager.ColorCodes[i].Xyz, Colors[i].Xyz);
            }
            AnimationModelLoader.Paint(this.Model, _modelPath, colorMap);
        }

        private void Load(Humanoid Human, HumanoidModelTemplate Template){
			this.Human = Human;
			this.Scale = Vector3.One * Template.Scale;
			this.Tint = Vector4.One;
            this.LeftWeapon = Weapon.Empty;
            this._modelPath = Template.Path;
			
			Model = AnimationModelLoader.LoadEntity(Template.Path);

            this.Model.Scale = DefaultScale * new Vector3(1, 1.15f, 1) * Template.Scale;
			this.LeftWeaponJoint = Model.RootJoint.GetChild("Hand_L");
			this.RightWeaponJoint = Model.RootJoint.GetChild("Hand_R");
			this.ChestJoint = Model.RootJoint.GetChild("Chest");
			this.LeftFootJoint = Model.RootJoint.GetChild("Foot_L");
			this.RightFootJoint = Model.RootJoint.GetChild("Foot_R");
            this.HeadJoint = Model.RootJoint.GetChild("Head");
			
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

            RollAnimation.Loop = false;
            RollAnimation.Speed = 0.75f;
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
				
				Human.IsEating = false;
				this.Model.Animator.ExitBlend();
			};

            this.Human.SetHitbox(AssetManager.LoadHitbox(Template.Path) * this.Model.Scale);
            this.Height = 1.65f;//this.Human.DefaultBox.Max.Y - this.Human.DefaultBox.Min.Y;

            this.Idle();
            this.Model.Size = this.Human.BaseBox.Max - this.Human.BaseBox.Min; 
            this._modelSound = new AreaSound(SoundType.HumanRun, Vector3.Zero, 48f);
        }

        public void Resize(Vector3 Scalar)
        {
            this.Model.Scale *= Scalar;
            this.Human.MultiplyHitbox(Scalar);
        }
		
		public void UpdateModel(){}
		
		public void SetLamp(bool Active)
        {
			if(this._hasLamp == Active) return;
			this._hasLamp = Active;
			
			if(this._lampModel == null){
				var lampData = AssetManager.PlyLoader("Assets/Items/Handlamp.ply", new Vector3(1.5f, 1.5f, 1.5f), Vector3.Zero, Vector3.Zero, true);
	        	this._lampModel = ObjectMesh.FromVertexData(lampData);
			}
			this._lampModel.Enabled = Active;
		}

        public void DisposeAnimation()
        {
            Model.SwitchShader(AnimatedModel.DeathShader);
            Alpha = 0;
            DisposeTime = 0;
        }

        public float DisposeTime
        {
            get => Model.DisposeTime;
            set => Model.DisposeTime = value;
        }

        public void Recompose()
        {
            Model.SwitchShader(AnimatedModel.DefaultShader);
            DisposeTime = 0;
        }

        public override void Attack(Entity Target, float Damage)
		{
			if(!Human.Knocked && !Human.IsAttacking && !(Human is LocalPlayer)){
				LeftWeapon.Attack1(this.Human);
			}
		}
		
		public void SetWeapon(Weapon Weapon)
        {
			if(Weapon == this.LeftWeapon)
				return;

		    this.LeftWeapon.Dispose();
		    this.UnregisterModel(this.LeftWeapon);

			this.LeftWeapon = Weapon;
			this.LeftWeapon.Enabled = this.Enabled;
            this.LeftWeapon.Scale = Model.Scale;
            this.LeftWeapon.Alpha = Model.Alpha;

            this.RegisterModel(this.LeftWeapon);

		    (Human as LocalPlayer)?.Toolbar.SetAttackType(this.LeftWeapon);
		}
		
		public override void Run(){
		    Human.IsMoving = true;

            if (Human.IsRolling || Human.IsRiding  || Human.IsUnderwater || Human.IsAttacking || Human.IsGliding || Human.IsJumping)
				return;

		    if (Model != null && Model.Animator.AnimationPlaying != WalkAnimation)
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
		
		public void Eat(float FoodHealth)
		{
		    TaskManager.While( 
                () => this.Human.IsEating && !Human.IsDead,
                () => Human.Health += FoodHealth * Time.FrameTimeSeconds * .3f);
			this._foodHealth = FoodHealth;
			Model.Animator.StopBlend();
			Model.Animator.BlendAnimation(EatAnimation);
			this.Human.WasAttacking = false;
			this.Human.IsAttacking = false;
		}
		
		public void Swim()
        {
			if( !(Human is LocalPlayer) || Human.IsRolling || Human.IsCasting) return;
			if(Model != null && Model.Animator.AnimationPlaying != SwimAnimation)
				Model.PlayAnimation(SwimAnimation);
		}
		
		public void IdleSwim()
        {
			if( !(Human is LocalPlayer) || Human.IsRolling || Human.IsCasting) return;
			if(Model != null && Model.Animator.AnimationPlaying != IdleSwimAnimation)
				Model.PlayAnimation(IdleSwimAnimation);
		}
		
		public void Roll()
        {
			Model.Animator.PlayAnimation(RollAnimation);
			this.Human.WasAttacking = false;
			this.Human.IsAttacking = false;
		}
		
		public void Tied()
        {
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
            base.Update();

		    WalkAnimation.Speed = Human.Speed;
		    this._modelSound.Pitch = Human.Speed / 1.11f;
            if (this.Model.Animator.AnimationPlaying != this.RollAnimation && Human.IsRolling)
		        Human.FinishRoll();

            if (_lastAnimationTime != this.Model.Animator.AnimationTime || GameManager.InMenu)
            {
				_lastAnimationTime = this.Model.Animator.AnimationTime;
			}
            else
            {
				Model.Animator.AnimationPlaying.DispatchEvents(1f);
				Model.Animator.StopBlend();
			}
			
			if(Model.Animator.AnimationPlaying == null)
				this.Idle();

			if(Model != null)
            {
				Model.Update();
				Vector3 PositionAddon = -Vector3.UnitY * 1.5f;
				if(this.MountModel != null)
					PositionAddon += (MountModel.Parent.HitBox.Max.Y - MountModel.Parent.HitBox.Min.Y) * Vector3.UnitY * 0.65f;
				
				Model.Position = this.Position + PositionAddon;
			    this._rotationQuaternion = Quaternion.Slerp(this._rotationQuaternion, QuaternionMath.FromEuler(this.TargetRotation * Mathf.Radian), Time.unScaledDeltaTime * 6f);
			    Model.Rotation = QuaternionMath.ToEuler(this._rotationQuaternion);

                this.Rotation = Model.Rotation;
				//this._shadow.Position = Model.Position;
				if(MountModel != null)
					this.MountModel.TargetRotation = this.TargetRotation;
			}
		
			if(Human.IsEating && this.Model.Animator.BlendingAnimation != EatAnimation){
				Human.IsEating = false;
				this.Food.Enabled = false;
			}else if(Human.IsEating){
				Matrix4 Mat4 = this.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + ((this.LeftWeaponPosition + this.RightWeaponPosition) / 2f) );
				
				this.Food.Model.TransformationMatrix = Mat4;
				this.Food.Model.Position = Model.Position;
				this.Food.Model.TargetPosition = Vector3.Zero;
				this.Food.Model.AnimationPosition = Vector3.Zero;
				this.Food.Model.TargetRotation = new Vector3(180,0,0);
				this.Food.Model.RotationPoint = Vector3.Zero;
				this.Food.Model.Rotation = Vector3.Zero;
				this.Food.Model.LocalRotation = Vector3.Zero;
				this.Food.Model.LocalPosition = Vector3.Zero;
				this.Food.Model.BeforeLocalRotation = Vector3.UnitY * -0.7f;
				
				if(!this.Human.IsMoving && Model.Animator.AnimationPlaying == WalkAnimation){
					this.Model.Animator.ExitBlend();
				}
			}
			
			if(!LockWeapon) this.LeftWeapon.Update(this.Human);
			
			if(this._hasLamp){
				this._lampModel.Position = this.LeftWeaponPosition;
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
				    World.Particles.VariateUniformly = true;
				    World.Particles.Color = Vector4.One;//World.GetHighestBlockAt( (int) this.Human.Position.X, (int) this.Human.Position.Z).GetColor(Region.Default);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
				    World.Particles.Position = this.Human.Position - Vector3.UnitY;
				    World.Particles.Scale = Vector3.One * .5f;
				    World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				    World.Particles.Direction = (-this.Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
				    World.Particles.ParticleLifetime = 1;
				    World.Particles.GravityEffect = .1f;
				    World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
					for(int i = 0; i < 1; i++){
					    World.Particles.Emit();
					}
				}
				_previousPosition = this.Human.BlockPosition;
			}
		    if (MountModel != null)
		    {
		        //Adapt human to animal
		        // this.Model.TransformationMatrix = Matrix4.CreateTranslation(Vector3.UnitY * MountModel.Height)
		        //* MountModel.Model.TransformationMatrix
		        //* Matrix4.CreateTranslation(Vector3.UnitY * -MountModel.Height);
		    }

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

        public override void Draw()
        {
			if(!Enabled)
				return;
			this.Model.Draw();
			if(this.LeftWeapon.Meshes != null){
				for(int i = 0; i < this.LeftWeapon.Meshes.Length; i++){
					this.LeftWeapon.Meshes[i].Draw();
				}
			}
		}
		
		public override float Alpha {
			get => _targetAlpha;
		    set => _targetAlpha = value;
		}

        public Matrix4 ChestMatrix => Model.MatrixFromJoint(ChestJoint);

        public Matrix4 LeftWeaponMatrix => Model.MatrixFromJoint(LeftWeaponJoint);

	    public Matrix4 RightWeaponMatrix => Model.MatrixFromJoint(RightWeaponJoint);

        private Vector3 _defaultHeadPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 HeadPosition
        {
            get
            {
                if (this._defaultHeadPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    this._defaultHeadPosition = Model.JointDefaultPosition(HeadJoint);
                return Model.TransformFromJoint(this._defaultHeadPosition, HeadJoint);
            }
        }

        private Vector3 _defaultLeftWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 LeftWeaponPosition{
			get{
				if(this._defaultLeftWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultLeftWeaponPosition = Model.JointDefaultPosition(LeftWeaponJoint);
				return Model.TransformFromJoint(this._defaultLeftWeaponPosition, LeftWeaponJoint);
			}
		}
		
		private Vector3 _defaultRightFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 RightFootPosition{
			get{
				if(this._defaultRightFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultRightFootPosition = Model.JointDefaultPosition(RightFootJoint);
				return Model.TransformFromJoint(this._defaultRightFootPosition, RightFootJoint);
			}
		}
		
		private Vector3 _defaultLeftFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 LeftFootPosition{
			get{
				if(this._defaultLeftFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultLeftFootPosition = Model.JointDefaultPosition(LeftFootJoint);
				return Model.TransformFromJoint(this._defaultLeftFootPosition, LeftFootJoint);
			}
		}
		
		private Vector3 _defaultRightWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 RightWeaponPosition{
			get{
				if(this._defaultRightWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					this._defaultRightWeaponPosition = Model.JointDefaultPosition(RightWeaponJoint);
				return Model.TransformFromJoint(this._defaultRightWeaponPosition, RightWeaponJoint);
			}
		}

        private Vector3 _defaultChestPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 ChestPosition
        {
            get
            {
                if (this._defaultChestPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    this._defaultChestPosition = Model.JointDefaultPosition(ChestJoint);
                return Model.TransformFromJoint(this._defaultChestPosition, ChestJoint);
            }
        }
		
		private Vector3 _position;
		public override Vector3 Position{
			get => _position;
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
            _lampModel?.Dispose();
			LeftWeapon.Dispose();
			
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var field in this.GetType().GetFields(flags))
			{
				if(field.GetType() == typeof(Animation))
					(field.GetValue(this) as Animation)?.Dispose();
			}
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
        Villager,
        Mage
	}
}
