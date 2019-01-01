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
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using OpenTK;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc />
    /// <summary>
    /// Description of SheepModel.
    /// </summary>
    public sealed class QuadrupedModel : AnimatedUpdatableModel, IMountable, IAudible, IDisposeAnimation
    {    
        public override bool IsWalking => Array.IndexOf(WalkAnimations, this.Model.AnimationPlaying) != -1;
        public override bool IsIdling => Array.IndexOf(IdleAnimations, this.Model.AnimationPlaying) != -1;
        public bool AlignWithTerrain { get; set; }
        public bool IsMountable { get; set; }
        public IHumanoid Rider { get; set; }
        public bool HasRider => Rider != null;
        public Animation[] AttackAnimations { get; }
        private Animation[] IdleAnimations { get; }
        private Animation[] WalkAnimations { get; }
        private AttackEvent[] AttackAnimationsEvents { get; }
        private AnimatedCollider Collider { get; }
        private readonly float[] _walkAnimationSpeed;
        private readonly float _originalMobSpeed;
        private float _lastMobSpeed;

        public override CollisionShape BroadphaseCollider => Collider.Broadphase;
        public override CollisionShape HorizontalBroadphaseCollider => Collider.HorizontalBroadphase;
        public override CollisionShape[] Colliders => Collider.Shapes;
        public override Vector3[] Vertices => Collider.Vertices;
        protected override string ModelPath { get; set; }
        private float _attackCooldown;
        private Quaternion _targetTerrainOrientation = Quaternion.Identity;
        private Quaternion _terrainOrientation = Quaternion.Identity;
        private Quaternion _quaternionModelRotation = Quaternion.Identity;
        private Quaternion _quaternionTargetRotation;
        private Vector3 _eulerTargetRotation;
        private Vector3 _rotation;
        private Vector3 _position;
        private readonly bool _hasAnimationEvent;
        private readonly AreaSound _sound;


        public QuadrupedModel(Entity Parent, ModelTemplate Template) : base(Parent)
        {
            var rng = new Random(Parent.Seed);

            ModelPath = Template.Path;
            Model = AnimationModelLoader.LoadEntity(Template.Path);
            WalkAnimations = new Animation[Template.WalkAnimations.Length];
            IdleAnimations = new Animation[Template.IdleAnimations.Length];
            AttackAnimations = new Animation[Template.AttackAnimations.Length];
            AttackAnimationsEvents = new AttackEvent[Template.AttackAnimations.Length];
            _walkAnimationSpeed = new float[WalkAnimations.Length];
            _originalMobSpeed = Parent.Speed;

            this.AlignWithTerrain = Template.AlignWithTerrain;
            this.Model.Scale = Vector3.One * (Template.Scale + Template.Scale * rng.NextFloat() * .3f - Template.Scale * rng.NextFloat() * .15f);
            this.BaseBroadphaseBox = AssetManager.LoadHitbox(Template.Path) * this.Model.Scale;
            this.Dimensions = AssetManager.LoadDimensions(Template.Path) * this.Model.Scale;

            for (var i = 0; i < IdleAnimations.Length; i++)
            {
                IdleAnimations[i] = AnimationLoader.LoadAnimation(Template.IdleAnimations[i].Path);
                IdleAnimations[i].Speed = Template.IdleAnimations[i].Speed;
            }

            for (var i = 0; i < WalkAnimations.Length; i++)
            {
                WalkAnimations[i] = AnimationLoader.LoadAnimation(Template.WalkAnimations[i].Path);
                WalkAnimations[i].Speed = Template.WalkAnimations[i].Speed;
                _walkAnimationSpeed[i] = WalkAnimations[i].Speed;
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
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationStart).Build();
                    };
                }
                if (Template.AttackAnimations[i].OnAnimationMid != null)
                {
                    _hasAnimationEvent = true;
                    AttackAnimations[i].OnAnimationMid += delegate
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationMid).Build();
                    };
                }
                if (Template.AttackAnimations[i].OnAnimationEnd != null)
                {
                    _hasAnimationEvent = true;
                    AttackAnimations[i].OnAnimationEnd += delegate
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationEnd).Build();
                    };
                }
                if (Template.AttackAnimations[i].OnAnimationProgress != null)
                {
                    _hasAnimationEvent = true;
                    AttackAnimations[i].RegisterOnProgressEvent(Template.AttackAnimations[k].OnAnimationProgress.Progress, delegate(Animation Sender)
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationProgress.Event).Build();
                    });
                }
                AttackAnimations[i].OnAnimationEnd += delegate {
                    this.IsAttacking = false;
                    this.Idle();
                };
                AttackAnimationsEvents[i] = (AttackEvent) Enum.Parse(typeof(AttackEvent), Template.AttackAnimations[i].AttackEvent);
            }
            this.Collider = new AnimatedCollider(Template.Path, Model);
            this.Idle();
            var soundType = Parent.MobType == MobType.Horse ? SoundType.HorseRun : SoundType.HumanRun;
            this._sound = new AreaSound(soundType, this.Position, 48f);
        }

        public bool CanAttack()
        {
            if (Array.IndexOf(AttackAnimations, Model.AnimationPlaying) != -1 || Parent.IsKnocked)
                return false;
            return _attackCooldown < 0;
        }

        public void Attack(IEntity Victim, Animation Animation, OnAnimationHandler Callback, float RangeModifier = 1.0f)
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
                        SoundPlayer.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
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

        public void Attack(Animation Animation, float RangeModifier)
        {
            this.Attack(null, Animation, null, RangeModifier);
        }

        public override void Attack(IEntity Victim)
        {
            this.Attack(Victim, SelectAttackAnimation(), null);
        }

        public override void Attack(IEntity Victim, float RangeModifier)
        {
            this.Attack(Victim, SelectAttackAnimation(), null, RangeModifier);
        }

        private void SetAttackHandler(Animation AttackAnimation, OnAnimationHandler Handler, bool Add)
        {
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
  
        public override void Update()
        {
            this.UpdateWalkAnimationsSpeed();
            if (!IsAttacking)
            {
                var isMoving = !HasRider 
                    ? Parent.IsMoving 
                    : Rider.IsMoving;
                if (isMoving) this.Run();
                else this.Idle();
            }
            base.Update();

            if (Model != null)
            {
                Model.Update();
                if (HasRider) TargetRotation = Rider.Model.TargetRotation;
                if (HasRider) _position = Rider.Model.ModelPosition - Rider.Model.RidingOffset;
                _targetTerrainOrientation = AlignWithTerrain
                    ? new Matrix3(
                        Mathf.RotationAlign(
                            Vector3.UnitY,
                            (
                                Physics.NormalAtPosition(this.Position) + 
                                Physics.NormalAtPosition(this.Position + new Vector3(Chunk.BlockSize, 0, 0)) +
                                Physics.NormalAtPosition(this.Position + new Vector3(0, 0, Chunk.BlockSize)) +
                                Physics.NormalAtPosition(this.Position + new Vector3(Chunk.BlockSize, 0, Chunk.BlockSize))
                            ) * .25f
                        )
                    ).ExtractRotation() 
                    : Quaternion.Identity;
                _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation, Time.IndependantDeltaTime * 8f);
                Model.TransformationMatrix = Matrix4.CreateFromQuaternion(_terrainOrientation);
                _quaternionModelRotation = Quaternion.Slerp(_quaternionModelRotation, _quaternionTargetRotation, Time.IndependantDeltaTime * 14f);
                Model.LocalRotation = _quaternionModelRotation.ToEuler();
                Model.Position = this.Position;
                this.LocalRotation = Model.LocalRotation;                
            }

            if (!base.Disposed)
            {
                _sound.Position = this.Position;
                _sound.Pitch = Parent.Speed / PitchSpeed;
                _sound.Update(this.IsWalking && Parent.IsGrounded);
            }
            _attackCooldown -= Time.IndependantDeltaTime;
        }

        private void UpdateWalkAnimationsSpeed()
        {
            if(Math.Abs(_lastMobSpeed - Parent.Speed) < 0.005f) return;
            for (var i = 0; i < WalkAnimations.Length; i++)
            {
                WalkAnimations[i].Speed = (_walkAnimationSpeed[i] / _originalMobSpeed) * Parent.Speed;
            }
            _lastMobSpeed = Parent.Speed;
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

        private Animation SelectWalkingAnimation()
        {
            return WalkAnimations[Utils.Rng.Next(0, WalkAnimations.Length)];
        }
        
        private Animation SelectIdleAnimation()
        {
            return IdleAnimations[Utils.Rng.Next(0, WalkAnimations.Length)];
        }

        private Animation SelectAttackAnimation()
        {
            return AttackAnimations[Utils.Rng.Next(0, AttackAnimations.Length)];
        }
        
        private void Run()
        {
        
            if(this.IsAttacking) return;
            
            if(Model != null && !this.IsWalking)
                Model.PlayAnimation(SelectWalkingAnimation());
        }

        private void Idle()
        {
            if(this.IsAttacking) return;
            
            if(Model != null && !this.IsIdling)
                Model.PlayAnimation(SelectIdleAnimation());
        }
        
        public override void Draw()
        {
            this.Model.Draw();
        }

        public override Vector3 Position
        {
            get => _position;
            set
            {
                /* If it has a rider, ignore other values */
                if (!HasRider)
                    _position = value;
            }
        }

        public override Vector3 TargetRotation
        {
            get => _eulerTargetRotation;
            set
            {
                /* If it has a rider, ignore other values */
                if (HasRider) value = Rider.Model.TargetRotation;
                _eulerTargetRotation = value;
                _quaternionTargetRotation = QuaternionMath.FromEuler(_eulerTargetRotation * Mathf.Radian);
            }
        }
        public override Vector3 LocalRotation
        {
            get => _rotation;
            set
            {
                this._rotation = value;
                
                if( float.IsNaN(this._rotation.X) || float.IsNaN(this._rotation.Y) || float.IsNaN(this._rotation.Z))
                    this._rotation = Vector3.Zero;
            }
        }

        public override void Dispose()
        {
            this.IdleAnimations.ToList().ForEach(A => A.Dispose());
            this.WalkAnimations.ToList().ForEach(A => A.Dispose());
            this.AttackAnimations.ToList().ForEach(A => A.Dispose());
            this.Collider.Dispose();
            this.Model.Dispose();
            base.Dispose();
        }
    }
}
