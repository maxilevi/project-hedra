/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/05/2016
 * Time: 10:17 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.PhysicsSystem;
using OpenTK;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;

namespace Hedra.Engine.EntitySystem
{
	/// <inheritdoc />
	/// <summary>
	/// Description of SheepModel.
	/// </summary>
	public sealed class QuadrupedModel : UpdatableModel<AnimatedModel>, IMountable, IAudible, IDisposeAnimation
    {	
        public override bool IsWalking => this.Model.Animator.AnimationPlaying == this.WalkAnimation;
        public override bool IsIdling => Array.IndexOf(IdleAnimations, this.Model.Animator.AnimationPlaying) != -1;
        public bool AlignWithTerrain { get; set; }
        public bool IsMountable { get; set; }
        public Animation[] IdleAnimations { get; }
        public Animation WalkAnimation { get; }
        public Animation[] AttackAnimations { get; }
		public AttackEvent[] AttackAnimationsEvents { get; }
        public AnimatedCollider Collider { get; }

        public override CollisionShape BroadphaseCollider => Collider.Broadphase;
        public override CollisionShape[] Colliders => Collider.Shapes;
        public override Vector3[] Vertices => Collider.Vertices;
        private float _targetGain = 1f;
        private float _attackCooldown;
        private Quaternion _targetTerrainOrientation = Quaternion.Identity;
        private Quaternion _terrainOrientation = Quaternion.Identity;
        private Quaternion _quaternionModelRotation = Quaternion.Identity;
        private Quaternion _quaternionTargetRotation;
        private Vector3 _eulerTargetRotation;
        private Vector3 _rotation;
        private readonly bool _hasAnimationEvent;
        private readonly AreaSound _sound;


		public QuadrupedModel(Entity Parent, ModelTemplate Template) : base(Parent)
		{
            var rng = new Random(Parent.MobSeed);

			Model = AnimationModelLoader.LoadEntity(Template.Path);
			WalkAnimation = AnimationLoader.LoadAnimation(Template.WalkAnimation.Path);
			IdleAnimations = new Animation[Template.IdleAnimations.Length];
			AttackAnimations = new Animation[Template.AttackAnimations.Length];
			AttackAnimationsEvents = new AttackEvent[Template.AttackAnimations.Length];
		    WalkAnimation.Speed = Template.WalkAnimation.Speed;

		    this.AlignWithTerrain = Template.AlignWithTerrain;
            this.Model.Scale = Vector3.One * (Template.Scale + Template.Scale * rng.NextFloat() * .3f - Template.Scale * rng.NextFloat() * .15f);
			this.BaseBroadphaseBox = AssetManager.LoadHitbox(Template.Path) * this.Model.Scale;

			for (var i = 0; i < IdleAnimations.Length; i++)
		    {
				IdleAnimations[i] = AnimationLoader.LoadAnimation(Template.IdleAnimations[i].Path);
				IdleAnimations[i].Speed = Template.IdleAnimations[i].Speed;
			}

		    for (var i = 0; i < AttackAnimations.Length; i++)
		    {
		        AttackAnimations[i] = AnimationLoader.LoadAnimation(Template.AttackAnimations[i].Path);
		        AttackAnimations[i].Speed = Template.AttackAnimations[i].Speed;
                AttackAnimations[i].Loop = false;

		        int k = i;
                if (Template.AttackAnimations[i].OnAnimationStart != null)
		        {
		            _hasAnimationEvent = true;
                    AttackAnimations[i].OnAnimationStart += delegate
		            {
		                AnimationEventBuilder.Build(Parent, Template.AttackAnimations[k].OnAnimationStart).Build();
		            };
		        }
		        if (Template.AttackAnimations[i].OnAnimationMid != null)
		        {
		            _hasAnimationEvent = true;
                    AttackAnimations[i].OnAnimationMid += delegate
		            {
		                AnimationEventBuilder.Build(Parent, Template.AttackAnimations[k].OnAnimationMid).Build();
		            };
		        }
		        if (Template.AttackAnimations[i].OnAnimationEnd != null)
		        {
		            _hasAnimationEvent = true;
                    AttackAnimations[i].OnAnimationEnd += delegate
		            {
		                AnimationEventBuilder.Build(Parent, Template.AttackAnimations[k].OnAnimationEnd).Build();
		            };
		        }
		        AttackAnimations[i].OnAnimationEnd += delegate {
		            this.IsAttacking = false;
		            this.Idle();
                };
				AttackAnimationsEvents[i] = (AttackEvent) Enum.Parse(typeof(AttackEvent), Template.AttackAnimations[i].AttackEvent);
            }
            this.Collider = new AnimatedCollider(Template.Path, Model);
            this.Model.Size = this.BaseBroadphaseBox.Max - this.BaseBroadphaseBox.Min;
			this.Idle();

		    var soundType = Parent.MobType == MobType.Horse ? SoundType.HorseRun : SoundType.HumanRun;
		    this._sound = new AreaSound(soundType, this.Position, 48f);
        }
		
		public void Resize(Vector3 Scalar){
			this.Model.Scale *= Scalar;
		    this.BaseBroadphaseBox *= Scalar;
        }

        public bool CanAttack()
        {
            if (Array.IndexOf(AttackAnimations, Model.Animator.AnimationPlaying) != -1 || Parent.Knocked)
                return false;
            return _attackCooldown < 0;
        }

        public void Attack(Entity Victim, Animation Animation, OnAnimationHandler Callback, float RangeModifier = 1.0f)
		{	
            if(!this.CanAttack())return;
		    var selectedAnimation = Animation;

		    if (!_hasAnimationEvent || Callback != null)
		    {
		        void AttackHandler(Animation Sender)
		        {
					this.SetAttackHandler(selectedAnimation, AttackHandler, false);
		            if (!Parent.InAttackRange(Victim, RangeModifier))
		            {
		                SoundManager.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
		                return;
		            }

		            Victim.Damage(Parent.AttackDamage, this.Parent, out float exp);
		        }
				this.SetAttackHandler(selectedAnimation, Callback ?? AttackHandler, true);
            }
		    Model.PlayAnimation(selectedAnimation);
		    IsAttacking = true;
		    _attackCooldown = Parent.AttackCooldown;
        }

        public void Attack(Animation Animation)
        {
            this.Attack(null, Animation, null);
        }

        public override void Attack(Entity Victim)
        {
            this.Attack(Victim, AttackAnimations[Utils.Rng.Next(0, AttackAnimations.Length)], null);
        }

        public override void Attack(Entity Victim, float RangeModifier)
        {
            this.Attack(Victim, AttackAnimations[Utils.Rng.Next(0, AttackAnimations.Length)], null, RangeModifier);
        }

		private void SetAttackHandler(Animation AttackAnimation, OnAnimationHandler Handler, bool Add){
			switch(AttackAnimationsEvents[Array.IndexOf(AttackAnimations, AttackAnimation)]){
				case AttackEvent.Start:
					if(Add)
						AttackAnimation.OnAnimationStart += Handler;
					else
						AttackAnimation.OnAnimationStart -= Handler;
					break;
				case AttackEvent.Mid:
					if(Add)
						AttackAnimation.OnAnimationMid += Handler;
					else
						AttackAnimation.OnAnimationMid -= Handler;
					break;
				case AttackEvent.End:
					if(Add)
						AttackAnimation.OnAnimationEnd += Handler;
					else
						AttackAnimation.OnAnimationEnd -= Handler;
					break;
			}
		}

        public override void Update(){
            base.Update();

			if(Model.Animator.AnimationPlaying == null)
				this.Idle();

		    if (Model != null)
		    {
		        if (Model.Rendered)
		        {
		            Model.Update();
		        }

		        _targetTerrainOrientation = AlignWithTerrain ? new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(this.Position))) .ExtractRotation() : Quaternion.Identity;
		        _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation, Time.unScaledDeltaTime * 8f);
		        Model.TransformationMatrix = Matrix4.CreateFromQuaternion(_terrainOrientation);
		        _quaternionModelRotation = Quaternion.Slerp(_quaternionModelRotation, _quaternionTargetRotation, Time.unScaledDeltaTime * 14f);
		        Model.Rotation = _quaternionModelRotation.ToEuler();
		        Model.Position = this.Position;
                this.Rotation = Model.Rotation; 
		        
		    }

		    if (!base.Disposed)
		    {
		        _sound.Position = this.Position;
		        _sound.Update(this.IsWalking);
		    }
		    _attackCooldown -= Time.FrameTimeSeconds;
		}

        public void StopSound()
        {
            _sound.Stop();
        }

        public void DisposeAnimation()
        {
            Model.SwitchShader(AnimatedModel.DeathShader);
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

        public override void Run()
        {
		
			if(this.IsAttacking) return;
			
			if(Model != null && Model.Animator.AnimationPlaying != WalkAnimation)
				Model.PlayAnimation(WalkAnimation);
		}

		public override void Idle()
        {
			if(this.IsAttacking) return;
			
			if(Model != null && !this.IsIdling)
				Model.PlayAnimation(IdleAnimations[Utils.Rng.Next(0, IdleAnimations.Length)]);
		}
		
		public override void Draw()
        {
			this.Model.Draw();
		}

        public override Vector3 Position { get; set; }

        public override Vector3 TargetRotation
        {
            get => _eulerTargetRotation;
            set
            {
                _eulerTargetRotation = value;
                _quaternionTargetRotation = QuaternionMath.FromEuler(_eulerTargetRotation * Mathf.Radian);
            }
        }
		public override Vector3 Rotation
        {
			get => _rotation;
		    set{
				this._rotation = value;
				
				if( float.IsNaN(this._rotation.X) || float.IsNaN(this._rotation.Y) || float.IsNaN(this._rotation.Z))
					this._rotation = Vector3.Zero;
			}
		}

        public override void Dispose()
        {
            this.IdleAnimations.ToList().ForEach(A => A.Dispose());
            this.WalkAnimation.Dispose();
            this.AttackAnimations.ToList().ForEach(A => A.Dispose());
            this.Collider.Dispose();
            this.Model.Dispose();
            base.Dispose();
        }
	}
}
