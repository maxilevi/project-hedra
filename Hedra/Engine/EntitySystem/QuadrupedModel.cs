/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 21/05/2016
 * Time: 10:17 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.SkillSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of SheepModel.
    /// </summary>
    public sealed class QuadrupedModel : AnimatedUpdatableModel, IMountable, IAudible, IDisposeAnimation
    {
        private readonly bool[] _hasAnimationEvent;
        private readonly float _originalMobSpeed;
        private readonly AreaSound _sound;
        private readonly float[] _walkAnimationSpeed;
        private float _attackCooldown;
        private Vector3 _eulerTargetRotation;
        private float _lastMobSpeed;
        private Vector3 _position;
        private Quaternion _quaternionModelRotation = Quaternion.Identity;
        private Quaternion _quaternionTargetRotation;
        private Vector3 _rotation;
        private Quaternion _targetTerrainOrientation = Quaternion.Identity;
        private Quaternion _terrainOrientation = Quaternion.Identity;


        public QuadrupedModel(ISkilledAnimableEntity Parent, ModelTemplate Template) : base(Parent)
        {
            var rng = new Random(Parent.Seed);

            TemplateScale = Vector3.One * Template.Scale;
            BaseRotation = Template.BaseRotation;
            BaseOffset = Template.BaseOffsetY * Vector3.UnitY * Template.Scale;
            IsFlyingModel = Template.IsFlying;
            IsUndead = Template.IsUndead;
            ModelPath = Template.RandomPath(rng);
            Model = AnimationModelLoader.LoadEntity(ModelPath, false, Template.FlipNormals);
            WalkAnimations = new Animation[Template.WalkAnimations.Length];
            IdleAnimations = new Animation[Template.IdleAnimations.Length];
            AttackAnimations = new Animation[Template.AttackAnimations.Length];
            AttackAnimationsEvents = new AttackEvent[Template.AttackAnimations.Length];
            AttackTemplates = Template.AttackAnimations;
            _hasAnimationEvent = new bool[Template.AttackAnimations.Length];
            _walkAnimationSpeed = new float[WalkAnimations.Length];
            _originalMobSpeed = Parent.Speed;

            AlignWithTerrain = Template.AlignWithTerrain;
            Model.Scale = Vector3.One * (Template.Scale + Template.Scale * rng.NextFloat() * .3f -
                                         Template.Scale * rng.NextFloat() * .15f);
            BaseBroadphaseBox = AssetManager.LoadHitbox(ModelPath) * Model.Scale;
            Dimensions = AssetManager.LoadDimensions(ModelPath) * Model.Scale;

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

                var k = i;
                if (Template.AttackAnimations[i].OnAnimationStart != null)
                {
                    _hasAnimationEvent[i] = true;
                    AttackAnimations[i].OnAnimationStart += delegate
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationStart)
                            .Build();
                    };
                }

                if (Template.AttackAnimations[i].OnAnimationMid != null)
                {
                    _hasAnimationEvent[i] = true;
                    AttackAnimations[i].OnAnimationMid += delegate
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationMid)
                            .Build();
                    };
                }

                if (Template.AttackAnimations[i].OnAnimationEnd != null)
                {
                    _hasAnimationEvent[i] = true;
                    AttackAnimations[i].OnAnimationEnd += delegate
                    {
                        AnimationEventBuilder.Instance.Build(Parent, Template.AttackAnimations[k].OnAnimationEnd)
                            .Build();
                    };
                }

                if (Template.AttackAnimations[i].OnAnimationProgress != null)
                {
                    _hasAnimationEvent[i] = true;
                    AttackAnimations[i].RegisterOnProgressEvent(
                        Template.AttackAnimations[k].OnAnimationProgress.Progress,
                        delegate
                        {
                            AnimationEventBuilder.Instance
                                .Build(Parent, Template.AttackAnimations[k].OnAnimationProgress.Event).Build();
                        });
                }

                AttackAnimations[i].OnAnimationEnd += delegate
                {
                    IsAttacking = false;
                    Idle();
                };
                AttackAnimationsEvents[i] =
                    (AttackEvent)Enum.Parse(typeof(AttackEvent), Template.AttackAnimations[i].AttackEvent);
            }

            Collider = new AnimatedCollider(ModelPath, Model);
            Idle();
            var soundType = Parent.MobType == MobType.Horse ? SoundType.HorseRun : SoundType.HumanRun;
            _sound = new AreaSound(soundType, Position, 48f);
        }

        public override bool IsWalking => Array.IndexOf(WalkAnimations, Model.AnimationPlaying) != -1;
        public override bool IsIdling => Array.IndexOf(IdleAnimations, Model.AnimationPlaying) != -1;
        public bool AlignWithTerrain { get; set; }
        public bool IsFlyingModel { get; }
        public Vector3 TemplateScale { get; }
        public float BaseRotation { get; }
        public Vector3 BaseOffset { get;  }
        public bool HasRider => Rider != null;
        public AttackAnimationTemplate[] AttackTemplates { get; }
        public Animation[] AttackAnimations { get; }
        private Animation[] IdleAnimations { get; }
        private Animation[] WalkAnimations { get; }
        private AttackEvent[] AttackAnimationsEvents { get; }
        private AnimatedCollider Collider { get; }

        public override CollisionShape HorizontalBroadphaseCollider => Collider.HorizontalBroadphase;
        protected override string ModelPath { get; set; }

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
                if (value.IsInvalid())
                {
                    var a = 0;
                }
            }
        }

        public override Vector3 LocalRotation
        {
            get => _rotation;
            set
            {
                _rotation = value;

                if (float.IsNaN(_rotation.X) || float.IsNaN(_rotation.Y) || float.IsNaN(_rotation.Z))
                    _rotation = Vector3.Zero;
            }
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

        public bool IsMountable { get; set; }
        public IHumanoid Rider { get; set; }

        public bool CanAttack()
        {
            if (Array.IndexOf(AttackAnimations, Model.AnimationPlaying) != -1 || Parent.IsKnocked || Parent.IsStuck)
                return false;
            return _attackCooldown < 0;
        }

        public void Attack(IEntity Victim, Animation Animation, OnAnimationHandler Callback, float RangeModifier = 1.0f)
        {
            if (!CanAttack()) return;
            var selectedAnimation = Animation;
            var index = Array.IndexOf(AttackAnimations, Animation);

            if (!_hasAnimationEvent[index] || Callback != null)
            {
                void AttackHandler(Animation Sender)
                {
                    SetAttackHandler(selectedAnimation, AttackHandler, false);
                    if (!Parent.InAttackRange(Victim, RangeModifier))
                    {
                        SoundPlayer.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
                        return;
                    }

                    Victim.Damage(Parent.AttackDamage, Parent, out _);
                }

                SetAttackHandler(selectedAnimation, Callback ?? AttackHandler, true);
            }

            Model.PlayAnimation(selectedAnimation);
            IsAttacking = true;
            _attackCooldown = Parent.AttackCooldown;
        }


        public void Attack(Animation Animation, float RangeModifier)
        {
            Attack(null, Animation, null, RangeModifier);
        }

        public override bool CanAttack(IEntity Victim, float RangeModifier)
        {
            return CanAttack();
        }

        public override void Attack(IEntity Victim, float RangeModifier)
        {
            Attack(Victim, SelectAttackAnimation(), null, RangeModifier);
        }

        private void SetAttackHandler(Animation AttackAnimation, OnAnimationHandler Handler, bool Add)
        {
            switch (AttackAnimationsEvents[Array.IndexOf(AttackAnimations, AttackAnimation)])
            {
                case AttackEvent.Start:
                    if (Add)
                        AttackAnimation.OnAnimationStart += Handler;
                    else
                        AttackAnimation.OnAnimationStart -= Handler;
                    break;
                case AttackEvent.Mid:
                    if (Add)
                        AttackAnimation.OnAnimationMid += Handler;
                    else
                        AttackAnimation.OnAnimationMid -= Handler;
                    break;
                case AttackEvent.End:
                    if (Add)
                        AttackAnimation.OnAnimationEnd += Handler;
                    else
                        AttackAnimation.OnAnimationEnd -= Handler;
                    break;
            }
        }

        public override void BaseUpdate()
        {
            base.BaseUpdate();
            Model.Position = Position;
            Model.Update();
        }

        public override void Update()
        {
            UpdateWalkAnimationsSpeed();
            if (!IsAttacking)
            {
                var isMoving = !HasRider
                    ? Parent.IsMoving
                    : Rider.IsMoving;
                if (isMoving) Run();
                else Idle();
            }

            base.Update();

            if (Model != null)
            {
                if (HasRider)
                {
                    TargetRotation = Rider.Model.TargetRotation;
                    _position = Rider.Model.ModelPosition - Rider.Model.RidingOffset;
                }

                _targetTerrainOrientation = AlignWithTerrain
                    ? Mathf.RotationAlign(
                        Vector3.UnitY,
                        (
                            Physics.NormalAtPosition(Position) +
                            Physics.NormalAtPosition(Position + new Vector3(Chunk.BlockSize, 0, 0)) +
                            Physics.NormalAtPosition(Position + new Vector3(0, 0, Chunk.BlockSize)) +
                            Physics.NormalAtPosition(Position + new Vector3(Chunk.BlockSize, 0, Chunk.BlockSize))
                        ) * .25f
                    ).ExtractRotation()
                    : Quaternion.Identity;
                _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation,
                    Time.IndependentDeltaTime * 8f);
                Model.TransformationMatrix = Matrix4x4.CreateFromQuaternion(_terrainOrientation) * Matrix4x4.CreateRotationY(BaseRotation * Mathf.Radian) * Matrix4x4.CreateTranslation(BaseOffset);
                _quaternionModelRotation = Quaternion.Slerp(_quaternionModelRotation, _quaternionTargetRotation,
                    Time.IndependentDeltaTime * 14f);
                Model.LocalRotation = _quaternionModelRotation.ToEuler();
                if (HasRider)
                {
                    Model.LocalRotation = Rider.Model.LocalRotation;
                    Model.TransformationMatrix = Matrix4x4.CreateRotationY(-Model.LocalRotation.Y * Mathf.Radian)
                                                 * Rider.Model.TiltMatrix
                                                 * Matrix4x4.CreateRotationY(Model.LocalRotation.Y * Mathf.Radian);
                }

                LocalRotation = Model.LocalRotation;
            }

            if (!Disposed)
            {
                _sound.Position = Position;
                _sound.Pitch = Parent.Speed / PitchSpeed;
                _sound.Update(IsWalking && (Rider != null && Rider.IsGrounded || Parent.IsGrounded) && !IsFlyingModel);
            }

            _attackCooldown -= Time.IndependentDeltaTime;
        }

        private void UpdateWalkAnimationsSpeed()
        {
            if (Math.Abs(_lastMobSpeed - Parent.Speed) < 0.005f) return;
            for (var i = 0; i < WalkAnimations.Length; i++)
                WalkAnimations[i].Speed = _walkAnimationSpeed[i] / _originalMobSpeed * Parent.Speed;
            _lastMobSpeed = Parent.Speed;
        }

        private Animation SelectWalkingAnimation()
        {
            return WalkAnimations[Utils.Rng.Next(0, WalkAnimations.Length)];
        }

        private Animation SelectIdleAnimation()
        {
            return IdleAnimations[Utils.Rng.Next(0, IdleAnimations.Length)];
        }

        private Animation SelectAttackAnimation()
        {
            var rng = Utils.Rng.NextFloat();
            var length = AttackAnimations.Length;
            var offset = Utils.Rng.Next(0, length);
            for (var i = offset; i < offset + length; ++i)
            {
                var index = Mathf.Modulo(i, length);
                if (rng <= AttackTemplates[index].Chance)
                    return AttackAnimations[index];
                rng -= AttackTemplates[index].Chance;
            }

            throw new ArgumentOutOfRangeException("Failed to find suitable animation");
        }

        private void Run()
        {
            if (IsAttacking) return;

            if (Model != null && !IsWalking)
                Model.PlayAnimation(SelectWalkingAnimation());
        }

        private void Idle()
        {
            if (IsAttacking) return;

            if (Model != null && !IsIdling)
                Model.PlayAnimation(SelectIdleAnimation());
        }

        public override void Dispose()
        {
            IdleAnimations.ToList().ForEach(A => A.Dispose());
            WalkAnimations.ToList().ForEach(A => A.Dispose());
            AttackAnimations.ToList().ForEach(A => A.Dispose());
            Collider.Dispose();
            Model.Dispose();
            base.Dispose();
        }
    }
}