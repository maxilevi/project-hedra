﻿/*
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
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
    /// <inheritdoc/>
    /// <summary>
    /// Description of PlayerModel.
    /// </summary>
    public sealed class HumanoidModel : UpdatableModel<AnimatedModel>, IAudible, IDisposeAnimation
    {
	    private const float DefaultScale = 0.75f;
		public IHumanoid Human { get; private set; }
		private Animation _walkAnimation;
		private Animation _idleAnimation;
		private Animation _rollAnimation;
		private Animation _eatAnimation;
		private Animation _swimAnimation;
	    private Animation _idleSwimAnimation;
	    private Animation _rideAnimation;
	    private Animation _idleRideAnimation;
	    private Animation _climbAnimation;
		private Animation _sitAnimation;
		private Animation _glideAnimation;
		private Animation _knockedAnimation;
        private Animation _tiedAnimation;
        private Animation _sleepAnimation;
	    private Animation _jumpAnimation;
	    public Joint LeftShoulderJoint { get; private set; }
	    public Joint RightShoulderJoint { get; private set; }
	    public Joint LeftElbowJoint { get; private set; }
	    public Joint RightElbowJoint { get; private set; }
        public Joint LeftWeaponJoint { get; private set; }
        public Joint RightWeaponJoint { get; private set; }
        public Joint ChestJoint { get; private set; }
        public Joint LeftFootJoint { get; private set; }
        public Joint RightFootJoint { get; private set; }
        public Joint HeadJoint { get; private set; }
	    public Animation DefaultBlending { get; set; }
	    private Animation _animationPlaying;
        private AreaSound _modelSound;
		private StaticModel _food;
		public Weapon LeftWeapon { get; private set; }
		public QuadrupedModel MountModel;
	    private AnimatedCollider _collider;

        public override CollisionShape BroadphaseCollider => _collider.Broadphase;
        public override CollisionShape[] Colliders => _collider.Shapes;
        public override Vector3[] Vertices => _collider.Vertices;
        public bool LockWeapon { get; set; }
	    public override Vector4 Tint { get; set; }
	    public override Vector4 BaseTint { get; set; }
	    private string _modelPath;
        private ObjectMesh _lampModel;
	    private bool _hasLamp;
        private Vector3 _previousPosition;
        private Vector3 _rotation;
        private Quaternion _rotationQuaternionX;
        private Quaternion _rotationQuaternionY;
        private Quaternion _rotationQuaternionZ;
        private float _lastAnimationTime = -1;
        private float _alpha = 1f;
	    private Vector3 _defaultHeadPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	    private Vector3 _defaultLeftWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	    private Vector3 _defaultRightFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	    private Vector3 _defaultLeftFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	    private Vector3 _defaultRightWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
	    private Vector3 _defaultChestPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private bool _isDisposeAnimationPlaying;


        public HumanoidModel(IHumanoid Human, HumanoidModelTemplate Template) : base(Human)
		{
		    Load(Human, Template);
		}

        public HumanoidModel(IHumanoid Human, HumanType Type) : base(Human)
        {
            Load(Human, HumanoidLoader.ModelTemplater[Type]);
        }

        public HumanoidModel(IHumanoid Human) : base(Human)
        {
            Load(Human, HumanoidLoader.ModelTemplater[Human.Class]);
        }

        public void Paint(Vector4[] Colors)
        {
            if(Colors.Length > AssetManager.ColorCodes.Length)
                throw new ArgumentOutOfRangeException($"Provided amount of colors cannot be higher than the color codes.");

            var colorMap = new Dictionary<Vector3, Vector3>();
            for (var i = 0; i < Colors.Length; i++)
            {
                colorMap.Add(AssetManager.ColorCodes[i].Xyz, Colors[i].Xyz);
            }
            AnimationModelLoader.Paint(Model, _modelPath, colorMap);
        }

        private void Load(IHumanoid Humanoid, HumanoidModelTemplate Template)
        {
			Human = Humanoid;
			Scale = Vector3.One * Template.Scale;
			Tint = Vector4.One;
            LeftWeapon = Weapon.Empty;
            _modelPath = Template.Path;
			
			Model = AnimationModelLoader.LoadEntity(Template.Path);

            Model.Scale = DefaultScale * new Vector3(1, 1.15f, 1) * Template.Scale;
			LeftWeaponJoint = Model.RootJoint.GetChild("Hand_L");
			RightWeaponJoint = Model.RootJoint.GetChild("Hand_R");
			ChestJoint = Model.RootJoint.GetChild("Chest");
			LeftFootJoint = Model.RootJoint.GetChild("Foot_L");
			RightFootJoint = Model.RootJoint.GetChild("Foot_R");
            HeadJoint = Model.RootJoint.GetChild("Head");
	        LeftElbowJoint = Model.RootJoint.GetChild("Arm_L");
	        RightElbowJoint = Model.RootJoint.GetChild("Arm_R");
	        LeftShoulderJoint = Model.RootJoint.GetChild("Shoulder_L");
	        RightShoulderJoint = Model.RootJoint.GetChild("Shoulder_R");
			
			_walkAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWalk.dae");
			_idleAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
			_rollAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorRoll.dae");
			_eatAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorEat.dae");
			_swimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwim.dae");
			_idleSwimAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSwimIdle.dae");
			_climbAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorClimb.dae");
			_rideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMount.dae");
			_idleRideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorMountIdle.dae");
			_sitAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSit.dae");
			_glideAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorGlide.dae");
			_knockedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorKnocked.dae");
            _tiedAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorTied.dae");
            _sleepAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSleep.dae");
	        _jumpAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorJump.dae");
	        
			_rollAnimation.OnAnimationEnd += delegate
			{ 
				Humanoid.Physics.ResetFall();
			    Human.IsRolling = false;
			};

            _eatAnimation.OnAnimationStart += delegate
            {
                SoundManager.PlaySound(SoundType.FoodEat, Position);
            };

			_eatAnimation.OnAnimationEnd += delegate{ 
				_food.Enabled = false;			
				Humanoid.IsEating = false;
			};

            _collider = new AnimatedCollider(Template.Path, Model);
            BaseBroadphaseBox = AssetManager.LoadHitbox(Template.Path) * Model.Scale;
            Dimensions = AssetManager.LoadDimensions(Template.Path) * Model.Scale;
            _modelSound = new AreaSound(SoundType.HumanRun, Vector3.Zero, 48f);
	        _food = new StaticModel(VertexData.Empty)
	        {
		        Scale = Vector3.One * 1.5f
	        };
        }

		public void SetLamp(bool Active)
        {
			if(_hasLamp == Active) return;
			_hasLamp = Active;
			
			if(_lampModel == null){
				var lampData = AssetManager.PLYLoader("Assets/Items/Handlamp.ply", new Vector3(1.5f, 1.5f, 1.5f), Vector3.Zero, Vector3.Zero, true);
	        	_lampModel = ObjectMesh.FromVertexData(lampData);
			}
			_lampModel.Enabled = Active;
		}

        public void DisposeAnimation()
        {
            _isDisposeAnimationPlaying = true;
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
            _isDisposeAnimationPlaying = false;
            Model.SwitchShader(AnimatedModel.DefaultShader);
            DisposeTime = 0;
        }

        public override void Attack(IEntity Victim)
		{
			if(!Human.IsKnocked && !Human.IsAttacking && !(Human is LocalPlayer)){
				LeftWeapon.Attack1(Human);
			}
		}
		
		public void SetWeapon(Weapon Weapon)
        {
			if(Weapon == LeftWeapon)
				return;

		    LeftWeapon.Dispose();
		    UnregisterModel(LeftWeapon);

			LeftWeapon = Weapon;
			LeftWeapon.Enabled = Enabled;
            LeftWeapon.Scale = Model.Scale;
            LeftWeapon.Alpha = Model.Alpha;

            RegisterModel(LeftWeapon);

		    (Human as LocalPlayer)?.Toolbar.SetAttackType(LeftWeapon);
		}

	    public void SetFood(Item Food)
	    {
		    _food.SetModel(Food.Model);
	    }
		
		public void Eat(float FoodHealth)
		{
			Human.IsEating = true;
		    TaskManager.While( 
                () => Human.IsEating && !Human.IsDead,
                () => Human.Health += FoodHealth * Time.IndependantDeltaTime * .3f);
			Model.BlendAnimation(_eatAnimation);
			Human.WasAttacking = false;
			Human.IsAttacking = false;
		}

        private void HandleState()
        {
            Animation currentAnimation = null;
            var blendingAnimation = DefaultBlending;

            if (Human.IsMoving)
            {
                currentAnimation = _walkAnimation;
                Human.IsSitting = false;
                if (Human.IsRiding)
                {
                    currentAnimation = _rideAnimation;
                    MountModel?.Run();
                }
                if (Human.IsUnderwater)
                {
                    currentAnimation = _swimAnimation;
                }
            }
            else
            {
                currentAnimation = _idleAnimation;
                if (Human.IsRiding)
                {
                    currentAnimation = _idleRideAnimation;
                    MountModel?.Idle();
                }
                if (Human.IsUnderwater)
                {
                    currentAnimation = _idleSwimAnimation;
                }
            }
            if (Human.IsTied)
            {
                currentAnimation = _tiedAnimation;
            }
            if (Human.IsSitting)
            {
                currentAnimation = _sitAnimation;
            }
            if (Human.IsSleeping)
            {
                currentAnimation = _sleepAnimation;
            }
            if (Human.IsRolling)
            {
                currentAnimation = _rollAnimation;
                HandleRollEffects();
            }
            if (Human.IsJumping)
            {
                currentAnimation = _jumpAnimation;
            }
            if (Human.IsEating)
            {
                blendingAnimation = _eatAnimation;
                HandleEatingEffects();
            }
            if (Human.IsGliding)
            {
                currentAnimation = _glideAnimation;
            }
	        if (Human.IsClimbing)
	        {
		        currentAnimation = _climbAnimation;
	        }
            if (Human.IsKnocked)
            {
                currentAnimation = _knockedAnimation;
            }
            if (currentAnimation != null && Model.AnimationPlaying != currentAnimation 
                && (Model.AnimationPlaying != _animationPlaying || _animationPlaying == null))
		    {
			    Model.PlayAnimation(currentAnimation);
		    }
		    if (blendingAnimation != null && Model.AnimationBlending != blendingAnimation)
	        {
                if(!(blendingAnimation == DefaultBlending && Model.AnimationBlending != null))
                {
                    Model.BlendAnimation(blendingAnimation);
                }
            }
	    }
	    
	    public void PlayAnimation(Animation Animation)
	    {
		    _animationPlaying = Animation;
		    Model.PlayAnimation(_animationPlaying);
	    }

	    public void Blend(Animation Animation)
	    {
		    Model.BlendAnimation(Animation);
	    }

	    private void HandleEatingEffects()
	    {
		    var mat4 = LeftWeaponMatrix.ClearTranslation() * 
		    Matrix4.CreateTranslation(-Model.Position + ((LeftWeaponPosition + RightWeaponPosition) / 2f) );
				
		    _food.TransformationMatrix = mat4;
		    _food.Position = Model.Position;
		    _food.TargetPosition = Vector3.Zero;
		    _food.AnimationPosition = Vector3.Zero;
		    _food.TargetRotation = new Vector3(180, 0, 0);
		    _food.RotationPoint = Vector3.Zero;
		    _food.Rotation = Vector3.Zero;
		    _food.LocalRotation = Vector3.Zero;
		    _food.LocalPosition = Vector3.Zero;
		    _food.BeforeLocalRotation = Vector3.Zero;
	    }
	    
	    private void HandleRollEffects()
	    {
		    if(_previousPosition != Human.BlockPosition && Human.IsGrounded){
			    var block = World.GetHighestBlockAt( (int) Human.Position.X, (int) Human.Position.Z);
			    World.Particles.VariateUniformly = true;
			    World.Particles.Color = Vector4.One;//World.GetHighestBlockAt( (int) this.Human.Position.X, (int) this.Human.Position.Z).GetColor(Region.Default);// * new Vector4(.8f, .8f, 1.0f, 1.0f);
			    World.Particles.Position = Human.Position - Vector3.UnitY;
			    World.Particles.Scale = Vector3.One * .5f;
			    World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
			    World.Particles.Direction = (-Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
			    World.Particles.ParticleLifetime = 1;
			    World.Particles.GravityEffect = .1f;
			    World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
					
			    for(int i = 0; i < 1; i++){
				    World.Particles.Emit();
			    }
		    }
		    _previousPosition = Human.BlockPosition;
	    }
	    
        public override void Update()
		{
            base.Update();
            HandleState();
		    _walkAnimation.Speed = Human.Speed;
		    _modelSound.Pitch = Human.Speed / 1.11f;
			
			var positionAddon = Vector3.Zero;
			if (MountModel != null)
			{
				positionAddon += MountModel.Height * Vector3.UnitY * .5f;
			}

			Model.Position = Mathf.Lerp(Model.Position, Position + positionAddon, Time.IndependantDeltaTime * 32f);
			_rotationQuaternionX = Quaternion.Slerp(_rotationQuaternionX, QuaternionMath.FromEuler(Vector3.UnitX * TargetRotation * Mathf.Radian), Time.IndependantDeltaTime * 6f);
			_rotationQuaternionY = Quaternion.Slerp(_rotationQuaternionY, QuaternionMath.FromEuler(Vector3.UnitY * TargetRotation * Mathf.Radian), Time.IndependantDeltaTime * 6f);
			_rotationQuaternionZ = Quaternion.Slerp(_rotationQuaternionZ, QuaternionMath.FromEuler(Vector3.UnitZ * TargetRotation * Mathf.Radian), Time.IndependantDeltaTime * 6f);
			Model.Rotation = new Vector3(
				QuaternionMath.ToEuler(_rotationQuaternionX).X,
				QuaternionMath.ToEuler(_rotationQuaternionY).Y,
				QuaternionMath.ToEuler(_rotationQuaternionZ).Z
				);

			Rotation = Model.Rotation;
			//this._shadow.Position = Model.Position;
			if(MountModel != null)
				MountModel.TargetRotation = TargetRotation;

			if(!LockWeapon) LeftWeapon.Update(Human);
			
			if(_hasLamp){
				_lampModel.Position = LeftWeaponPosition;
				_lampModel.Rotation = Rotation;
				_lampModel.RotationPoint = Vector3.Zero;
			}
			Human.HandLamp.Update();

		    if (!Disposed)
		    {
		        _modelSound.Type = Human.IsSleeping ? SoundType.HumanSleep : SoundType.HumanRun;
		        _modelSound.Position = Position;
		        _modelSound.Update(IsWalking || Human.IsSleeping);
		    }
		    Model.Update();
        }

	    public void StopSound()
	    {
	        _modelSound.Stop();
	    }

        public override void Draw()
        {
			if(!Enabled) return;
			Model.Draw();
			if(LeftWeapon.Meshes != null)
			{
				for(var i = 0; i < LeftWeapon.Meshes.Length; i++)
				{
					LeftWeapon.Meshes[i].Draw();
				}
			}
		}

	    public Vector3 TransformFromJoint(Vector3 Point, Joint Joint)
	    {
		    return Model.TransformFromJoint(Point, Joint);
	    }
	    
	    public Vector3 JointDefaultPosition(Joint Joint)
	    {
		    return Model.JointDefaultPosition(Joint);
	    }
		
		public override float Alpha
        {
			get => _alpha;
		    set
		    {
		        _alpha = value;
		        Model.Enabled = Alpha > 0.005f || _isDisposeAnimationPlaying;
		    }
		}

        public Vector3 ModelPosition => Model.Position;

	    public Animation AnimationPlaying => Model.AnimationPlaying;
	    
	    public Animation AnimationBlending => Model.AnimationBlending;

        public Matrix4 ChestMatrix => Model.MatrixFromJoint(ChestJoint);

        public Matrix4 LeftWeaponMatrix => Model.MatrixFromJoint(LeftWeaponJoint);

	    public Matrix4 RightWeaponMatrix => Model.MatrixFromJoint(RightWeaponJoint);

		public Matrix4 LeftFootMatrix => Model.MatrixFromJoint(LeftFootJoint);

		public Matrix4 RightFootMatrix => Model.MatrixFromJoint(RightFootJoint);

        public Vector3 HeadPosition
        {
            get
            {
                if (_defaultHeadPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    _defaultHeadPosition = Model.JointDefaultPosition(HeadJoint);
                return Model.TransformFromJoint(_defaultHeadPosition, HeadJoint);
            }
        }

		public Vector3 LeftWeaponPosition
		{
			get
            {
				if(_defaultLeftWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					_defaultLeftWeaponPosition = Model.JointDefaultPosition(LeftWeaponJoint);
				return Model.TransformFromJoint(_defaultLeftWeaponPosition, LeftWeaponJoint);
			}
		}
		
		public Vector3 RightFootPosition
		{
			get
            {
				if(_defaultRightFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					_defaultRightFootPosition = Model.JointDefaultPosition(RightFootJoint);
				return Model.TransformFromJoint(_defaultRightFootPosition, RightFootJoint);
			}
		}
		
		public Vector3 LeftFootPosition
		{
			get
            {
				if(_defaultLeftFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					_defaultLeftFootPosition = Model.JointDefaultPosition(LeftFootJoint);
				return Model.TransformFromJoint(_defaultLeftFootPosition, LeftFootJoint);
			}
		}
		
		public Vector3 RightWeaponPosition
		{
			get
            {
				if(_defaultRightWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
					_defaultRightWeaponPosition = Model.JointDefaultPosition(RightWeaponJoint);
				return Model.TransformFromJoint(_defaultRightWeaponPosition, RightWeaponJoint);
			}
		}

        public Vector3 ChestPosition
        {
            get
            {
                if (_defaultChestPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    _defaultChestPosition = Model.JointDefaultPosition(ChestJoint);
                return Model.TransformFromJoint(_defaultChestPosition, ChestJoint);
            }
        }
		
		private Vector3 _position;
		public override Vector3 Position
		{
			get => _position;
		    set
            {
				if(MountModel != null)
					MountModel.Position = value;

                _position = value;
			}
		}

	    public Matrix4 TransformationMatrix
	    {
		    get => Model.TransformationMatrix;
		    set => Model.TransformationMatrix = value;
	    }
	    
	    public override Vector3 Rotation { get; set; }

	    public override void Dispose()
		{
		    _collider.Dispose();
            Model.Dispose();
            _lampModel?.Dispose();
			LeftWeapon.Dispose();
			
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var field in GetType().GetFields(flags))
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
