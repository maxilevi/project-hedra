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
	public class QuadrupedModel : Model, IMountable, IAudible, IDisposeAnimation
    {
		
		public bool IsAttacking { get; private set; }
        public bool AlignWithTerrain { get; set; } = true;
		private float _targetAlpha = 1f;
	    private float _targetGain = 1f;
        private float _attackCooldown;
        private Quaternion _targetTerrainOrientation = Quaternion.Identity;
        private Quaternion _terrainOrientation = Quaternion.Identity;
        private Quaternion _quaternionModelRotation = Quaternion.Identity;
        private readonly AreaSound _sound;

		public Entity Parent;
		public bool IsMountable {get; set;}
		public const float AttackCooldown = 1.5f;
		
		public AnimatedModel Model;
		public Animation IdleAnimation;
		public Animation WalkAnimation;
		public Animation[] AttackAnimations;

		public QuadrupedModel(Entity Parent, ModelTemplate Template)
		{
			this.Parent = Parent;
		    _attackCooldown = AttackCooldown;
            var rng = new Random(Parent.MobSeed);

			Model = AnimationModelLoader.LoadEntity(Template.Path);
			IdleAnimation = AnimationLoader.LoadAnimation(Template.IdleAnimation.Path);
			WalkAnimation = AnimationLoader.LoadAnimation(Template.WalkAnimation.Path);
			AttackAnimations = new Animation[Template.AttackAnimations.Length];

		    IdleAnimation.Speed = Template.IdleAnimation.Speed;
		    WalkAnimation.Speed = Template.WalkAnimation.Speed;

			this.Model.Scale = Vector3.One * (Template.Scale + Template.Scale * rng.NextFloat() * .3f - Template.Scale * rng.NextFloat() * .15f);
			this.Parent.SetHitbox(AssetManager.LoadHitbox(Template.IdleAnimation.Path) * this.Model.Scale);

		    for (var i = 0; i < AttackAnimations.Length; i++)
		    {
		        AttackAnimations[i] = AnimationLoader.LoadAnimation(Template.AttackAnimations[i].Path);
		        AttackAnimations[i].Speed = Template.AttackAnimations[i].Speed;
                AttackAnimations[i].Loop = false;
		        AttackAnimations[i].OnAnimationEnd += delegate {
		            this.IsAttacking = false;
		        };
            }
			
			this.Model.Size = (this.Parent.BaseBox.Max - this.Parent.BaseBox.Min);
			this.Idle();

		    var soundType = Parent.MobType == MobType.Horse ? SoundType.HorseRun : SoundType.HumanRun;
		    this._sound = new AreaSound(soundType, this.Position, 48f);

		}
		
		public void Resize(Vector3 Scalar){
			this.Model.Scale *= Scalar;
		    this.Parent.MultiplyHitbox(Scalar);
        }

		public override void Attack(Entity Damagee, float Damage)
		{	
			if(Array.IndexOf(AttackAnimations, Model.Animator.AnimationPlaying) != -1)
				return;
			
			if(_attackCooldown > 0){
				this.Idle();
				return;
			}

		    var selectedAnimation = AttackAnimations[Utils.Rng.Next(0, AttackAnimations.Length)];

		    void AttackHandler(Animation Sender)
		    {
		        if (!Parent.InAttackRange(Damagee)) return;
		        float exp;
		        Damagee.Damage(Damage, this.Parent, out exp);

		        selectedAnimation.OnAnimationMid -= AttackHandler;
		    }

		    selectedAnimation.OnAnimationMid += (OnAnimationHandler) AttackHandler;
			
			Model.PlayAnimation(selectedAnimation);
			this.IsAttacking = true;
		    _attackCooldown = AttackCooldown;
        }
		
		public override void Update(){
			if(Model.Animator.AnimationPlaying == null)
				this.Idle();
			
			if(base.Disposed) this.Model.Dispose();

		    if (Model != null)
		    {
		        if (Model.Rendered)
		            Model.Update();

		        _targetTerrainOrientation = AlignWithTerrain ? new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(this.Position))).ExtractRotation() : Quaternion.Identity;
		        _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation, Time.unScaledDeltaTime * 8f);
		        Model.TransformationMatrix = Matrix4.CreateFromQuaternion(_terrainOrientation);
                Model.Position = this.Position + Vector3.UnitY * 0.0f;
                _quaternionModelRotation = Quaternion.Slerp(_quaternionModelRotation, _quaternionTargetRotation,  Time.unScaledDeltaTime * 8f);
		        Model.Rotation = _quaternionModelRotation.ToEuler();
                this.Rotation = Model.Rotation;
                Model.BaseTint = Mathf.Lerp(Model.BaseTint, this.BaseTint, (float) Time.unScaledDeltaTime * 6f);
		        Model.Tint = Mathf.Lerp(Model.Tint, this.Tint, (float) Time.unScaledDeltaTime * 6f);
		        Model.Alpha = Mathf.Lerp(Model.Alpha, this._targetAlpha, (float) Time.ScaledFrameTimeSeconds * 8f);
		    }

		    if (!this.Disposed)
		    {
		        _sound.Position = this.Position;
		        _sound.Update(this.IsRunning);
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
            get { return Model.DisposeTime; }
            set { Model.DisposeTime = value; }
        }

        public void Recompose()
        {
            Model.SwitchShader(AnimatedModel.DefaultShader);
            DisposeTime = 0;
        }

        public override void Run(){
			
			if(this.IsAttacking)
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != WalkAnimation)
				Model.PlayAnimation(WalkAnimation);
		}
		public override void Idle(){
			if(this.IsAttacking)
				return;
			
			if(Model != null && Model.Animator.AnimationPlaying != IdleAnimation)
				Model.PlayAnimation(IdleAnimation);
		}
		
		public void Draw(){
			this.Model.Draw();
		}

	    private bool IsRunning => this.Model.Animator.AnimationPlaying == this.WalkAnimation;
	    private bool IsIdle => this.Model.Animator.AnimationPlaying == this.IdleAnimation;

        private Quaternion _quaternionTargetRotation;
        private Vector3 _eulerTargetRotation;
        public override Vector3 TargetRotation
        {
            get { return _eulerTargetRotation; }
            set
            {
                _eulerTargetRotation = value;
                _quaternionTargetRotation = QuaternionMath.FromEuler(_eulerTargetRotation * Mathf.Radian);
            }
        }

        public override float Alpha {
			get { return this._targetAlpha; }
			set {
				this._targetAlpha = value;				
			}
		}
		
		public override bool ApplyFog{
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
				Model.Enabled = value;
				_enabled = value;
			}
		}

        public override Vector3 Position { get; set; }

        private Vector3 _rotation;
		public override Vector3 Rotation{
			get{
				return _rotation;
			}
			set{
				this._rotation = value;
				
				if( float.IsNaN(this._rotation.X) || float.IsNaN(this._rotation.Y) || float.IsNaN(this._rotation.Z))
					this._rotation = Vector3.Zero;
			}
		}
	}
}
