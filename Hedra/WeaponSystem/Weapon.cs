/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 06/05/2016
 * Time: 09:17 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Hedra.Engine;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.WeaponSystem
{
    /// <summary>
    /// Description of Weapon.
    /// </summary>
    public abstract class Weapon : IModel
    {
        public ObjectMesh MainMesh { get; }
        public ObjectMesh[] Meshes { get; private set; }
        public VertexData MeshData { get; }
        public virtual bool IsChargeable => false;
        public bool IsCharging { protected get; set; }
        public Animation AttackStanceAnimation { get; private set; }
        public Animation ChargeStanceAnimation { get; private set; }
        protected virtual string ChargeStanceName { get; }
        protected virtual Vector3 SheathedPosition => new Vector3(-.6f, 0.5f, -0.8f);
        protected virtual Vector3 SheathedRotation => new Vector3(-5, 90, -135);
        protected virtual bool ContinousAttack => false;
        public abstract bool IsMelee { get; }
        public bool LockWeapon { get; set; } = false;
        private bool WeaponCoroutineExists;
        public bool Disposed { get; private set; }
        public Vector4 BaseTint { get; set; }
        public Vector4 Tint { get; set; }
        protected IHumanoid Owner { get; set; }
        protected bool SecondaryAttack { get; set; }
        protected bool PrimaryAttack { get; set; }
        protected int PrimaryAnimationsIndex { get; set; }
        private int SecondaryAnimationsIndex { get; set; }
        public bool Orientate { get; set; } = true;
        public SoundType SoundType { get; set; } = SoundType.SlashSound;
        public bool Charging { get; set; }
        public float ChargingIntensity { get; set; }
        protected virtual bool ShouldPlaySound { get; set; } = true;
        public float PrimaryAttackAnimationSpeed { get; set; } = 1.0f;
        public float SecondaryAttackAnimationSpeed { get; set; } = 1.0f;     
        public virtual bool PrimaryAttackEnabled { get; set; } = true;       
        public virtual bool SecondaryAttackEnabled { get; set; } = true;

        private Animation[] _secondaryAnimations;
        private Animation[] _primaryAnimations;
        private Animation[] _animations;
        private float[] _animationSpeeds;
        private EffectDescriber _describer;
        private bool _onAttackStance;
        private Vector3 _scale = Vector3.One;
        private float _alpha = 1f;
        private float _animationSpeed = 1f;
        private bool _applyFog;
        private bool _pause;
        private float _passedTimeInAttackStance;
        private bool _outline;
        private Vector4 _outlineColor;
        private bool _effectApplied;
        private AttackOptions _currentAttackOption;
        
        protected abstract void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options);
        protected abstract void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options);
        protected abstract string AttackStanceName { get; }
        protected abstract string[] PrimaryAnimationsNames { get; }
        protected abstract string[] SecondaryAnimationsNames { get; }
        protected abstract float PrimarySpeed { get; }
        protected abstract float SecondarySpeed { get; }
        public abstract uint PrimaryAttackIcon { get; }
        public abstract uint SecondaryAttackIcon { get; }

        protected Weapon(VertexData MeshData)
        {
            if (MeshData != null)
            {
                var baseMesh = MeshData.Clone();
                baseMesh.Scale(Vector3.One * 1.75f);
                MainMesh = ObjectMesh.FromVertexData(baseMesh);
                this.MeshData = baseMesh;
            }
            else
            {
                MainMesh = new ObjectMesh();
            }
            CreateAnimations();
        }

        private void CreateAnimations()
        {
            AttackStanceAnimation = AnimationLoader.LoadAnimation(AttackStanceName);
            ChargeStanceAnimation = AnimationLoader.LoadAnimation(ChargeStanceName ?? AttackStanceName);
            _primaryAnimations = new Animation[PrimaryAnimationsNames.Length];
            for (var i = 0; i < _primaryAnimations.Length; i++)
            {
                _primaryAnimations[i] = AnimationLoader.LoadAnimation(PrimaryAnimationsNames[i]);
                _primaryAnimations[i].Speed = PrimarySpeed;
                _primaryAnimations[i].Loop = false;
                _primaryAnimations[i].OnAnimationStart += 
                    Sender =>
                    {
                        Owner.InvokeBeforeAttackEvent(_currentAttackOption);
                        OnPrimaryAttackEvent(AttackEventType.Start, _currentAttackOption);
                    };
                _primaryAnimations[i].OnAnimationMid += 
                    Sender => OnPrimaryAttackEvent(AttackEventType.Mid, _currentAttackOption);
                _primaryAnimations[i].OnAnimationEnd += 
                    Sender =>
                    {
                        Owner.InvokeAfterAttackEvent(_currentAttackOption);
                        OnPrimaryAttackEvent(AttackEventType.End, _currentAttackOption);
                    };
            }
            
            _secondaryAnimations = new Animation[SecondaryAnimationsNames.Length];
            for (var i = 0; i < _secondaryAnimations.Length; i++)
            {
                _secondaryAnimations[i] = AnimationLoader.LoadAnimation(SecondaryAnimationsNames[i]);
                _secondaryAnimations[i].Speed = SecondarySpeed;
                _secondaryAnimations[i].Loop = false;
                _secondaryAnimations[i].OnAnimationStart += 
                    Sender =>
                    {
                        Owner.InvokeBeforeAttackEvent(_currentAttackOption);
                        OnSecondaryAttackEvent(AttackEventType.Start, _currentAttackOption);
                    };
                _secondaryAnimations[i].OnAnimationMid += 
                    Sender => OnSecondaryAttackEvent(AttackEventType.Mid, _currentAttackOption);
                _secondaryAnimations[i].OnAnimationEnd += 
                    Sender =>
                    {
                        Owner.InvokeAfterAttackEvent(_currentAttackOption);
                        OnSecondaryAttackEvent(AttackEventType.End, _currentAttackOption);
                    };
            }
            RegisterAnimationSpeeds();
        }

        private void RegisterAnimationSpeeds()
        {
            _animations = new Animation[_primaryAnimations.Length + _secondaryAnimations.Length];
            for (var i = 0; i < _primaryAnimations.Length; i++)
            {
                _animations[i] = _primaryAnimations[i];
            }
            for (var i = 0; i < _secondaryAnimations.Length; i++)
            {
                _animations[i + _primaryAnimations.Length] = _secondaryAnimations[i];
            }
            _animationSpeeds = new float[_animations.Length];
            for (var i = 0; i < _animations.Length; i++)
            {
                _animationSpeeds[i] = _animations[i].Speed;
            }
        }

        protected void SetToDefault(ObjectMesh Mesh)
        {
            Mesh.Position = Vector3.Zero;
            Mesh.LocalRotationPoint = Vector3.Zero;
            Mesh.LocalRotation = Vector3.Zero;
            Mesh.Rotation = Vector3.Zero;
            Mesh.RotationPoint = Vector3.Zero;
            Mesh.LocalPosition = Vector3.Zero;
            Mesh.BeforeRotation = Vector3.Zero;
            Mesh.TransformationMatrix = Matrix4.Identity;
        }

        protected void SetToChest(ObjectMesh Mesh)
        {
            SetToDefault(Mesh);
            Mesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() *
                                        Matrix4.CreateTranslation(
                                            -Owner.Position + Owner.Model.ChestPosition + Vector3.UnitY * 1f);
            Mesh.Position = Owner.Position;
            Mesh.Rotation = SheathedRotation;
            Mesh.BeforeRotation =
                (SheathedPosition + Vector3.UnitX * 2.25f + Vector3.UnitZ * 2.5f - Vector3.UnitY * 0.5f) * Scale;
        }

        protected void SetToMainHand(ObjectMesh Mesh)
        {
            Matrix4 Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() *
                           Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftWeaponPosition);

            Mesh.TransformationMatrix = Mat4;
            Mesh.Position = Owner.Position;
            Mesh.LocalRotationPoint = Vector3.Zero;
            Mesh.LocalRotation = Vector3.Zero;
            Mesh.Rotation = Vector3.Zero;
            Mesh.LocalPosition = Vector3.Zero;
            Mesh.BeforeRotation = Vector3.UnitY * -0.7f;
        }

        public virtual int ParsePrimaryIndex(int Index)
        {
            return Index;
        }

        public virtual int ParseSecondaryIndex(int Index)
        {
            return Index;
        }
        
        public void Attack1(IHumanoid Human)
        {
            Attack1(Human, new AttackOptions());
        }

        public virtual void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;

            PrimaryAnimationsIndex++;

            if (PrimaryAnimationsIndex == _primaryAnimations.Length)
                PrimaryAnimationsIndex = 0;

            BasePrimaryAttack(Human, Options);
        }

        protected void BasePrimaryAttack(IHumanoid Human, AttackOptions Options)
        {
            BaseAttack(Human, Options);
            var animation = _primaryAnimations[ParsePrimaryIndex(PrimaryAnimationsIndex)];
            animation.Speed = _animationSpeeds[Array.IndexOf(_animations, animation)] * Owner.AttackSpeed * PrimaryAttackAnimationSpeed;
            Human.Model.BlendAnimation(animation);
        }

        public void Attack2(IHumanoid Human)
        {
            Attack2(Human, new AttackOptions());
        }
        
        public virtual void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if (!MeetsRequirements()) return;

            SecondaryAnimationsIndex++;

            if (SecondaryAnimationsIndex == _secondaryAnimations.Length)
                SecondaryAnimationsIndex = 0;

            BaseSecondaryAttack(Human, Options);
        }

        protected void BaseSecondaryAttack(IHumanoid Human, AttackOptions Options)
        {
            BaseAttack(Human, Options);
            var animation = _secondaryAnimations[ParseSecondaryIndex(SecondaryAnimationsIndex)];
            animation.Speed = _animationSpeeds[Array.IndexOf(_animations, animation)] * Owner.AttackSpeed * SecondaryAttackAnimationSpeed;
            Human.Model.BlendAnimation(animation);
        }

        protected bool MeetsRequirements()
        {
            return Owner != null &&
                   !(Owner.IsAttacking || Owner.IsKnocked || SecondaryAttack || PrimaryAttack);
        }

        public void PlaySound()
        {
            if (Owner != null)
                SoundPlayer.PlaySoundWithVariation(SoundType, Owner.Position);
        }

        protected void BaseAttack(IHumanoid Human, AttackOptions Options)
        {
            Owner = Human;
            
            _currentAttackOption = Options;
            if (ShouldPlaySound && !IsMelee)
                SoundPlayer.PlaySoundWithVariation(SoundType, Human.Position);

            if (IsMelee && Owner.Movement.IsMovingBackwards)
            {
                TaskScheduler.DelayFrames(1,
                    () => TaskScheduler.While(
                        () => Human.IsAttacking/*Human.Model.IsWalking && !Human.IsMoving*/,
                        delegate
                        {
                            Human.Movement.Orientate();
                            Human.Physics.Move(Options.IdleMovespeed);
                        }
                    )
                );
            }
            else
            {
                TaskScheduler.DelayFrames(1, 
                    () => TaskScheduler.While(() => Human.IsAttacking,
                        () => Human.Physics.Move(Options.RunMovespeed)
                    )
                );
            }
            if (Orientate) Human.Movement.Orientate();
            StartWasAttackingCoroutine();
        }

        public virtual void Update(IHumanoid Human)
        {
            GatherMembers();
            Owner = Human;

            var attacking = false;
            for (var i = 0; i < _animations.Length; i++)
            {
                if (_animations[i] != Owner.Model.AnimationPlaying &&
                    _animations[i] != Owner.Model.AnimationBlending) continue;
                attacking = true;
                break;
            }

            var primaryAttack = false;
            for (var i = 0; i < _primaryAnimations.Length; i++)
            {
                if (_primaryAnimations[i] != Owner.Model.AnimationPlaying &&
                    _primaryAnimations[i] != Owner.Model.AnimationBlending) continue;
                primaryAttack = true;
                break;
            }
            PrimaryAttack = primaryAttack;

            var secondaryAttack = false;
            for (int i = 0; i < _secondaryAnimations.Length; i++)
            {
                if (_secondaryAnimations[i] == Owner.Model.AnimationPlaying ||
                    _secondaryAnimations[i] == Owner.Model.AnimationBlending)
                {
                    secondaryAttack = true;
                    break;
                }
            }
            SecondaryAttack = secondaryAttack;

            if (!attacking && Owner.Model.Human.IsAttacking)
            {
                Owner.Model.Human.WasAttacking = true;
                RoutineManager.StartRoutine(WasAttackingCoroutine);
            }

            Owner.IsAttacking = attacking;
            SetToDefault(MainMesh);
            
            if (Sheathed)
                OnSheathed();           

            if (InAttackStance || Owner.WasAttacking || (ContinousAttack && Owner.IsAttacking))
                OnAttackStance();
            
            if (PrimaryAttack)
                OnPrimaryAttack();

            if (SecondaryAttack)
                OnSecondaryAttack();
            
            if (PrimaryAttack || SecondaryAttack)
                OnAttack();

            if (Owner.IsAttacking)
            {
                Owner.Movement.Orientate();
            }

            ApplyEffects(MainMesh);

            if (Describer != null && Describer.Type != EffectType.None)
            {
                if (!_effectApplied)
                {
                    Owner.ApplyEffectWhile(Describer.Type, () => Owner.LeftWeapon == this);
                    _effectApplied = true;
                }
            }
        }
                  
        protected virtual void OnAttackStance()
        {
            if(IsCharging)
                OnChargeStance();
            else
                OnPostAttackStance();               
        }

        protected virtual void OnChargeStance()
        {
            
        }

        protected virtual void OnPostAttackStance()
        {
            
        }

        protected virtual void OnSheathed()
        {
        }

        
        protected virtual void OnPrimaryAttack()
        {
        }
        
        protected virtual void OnSecondaryAttack()
        {
        }
        
        protected virtual void OnAttack()
        {
            if (Orientate) Owner.Movement.Orientate();
        }

        protected void ApplyEffects(ObjectMesh Mesh)
        {
            Mesh.BaseTint = BaseTint;
            Mesh.Tint = Tint;
            if (Describer != null && Describer.Type != EffectType.None)
            {
                Mesh.BaseTint = Describer.EffectColor;
            }

            if (Charging)
            {
                var time = Time.AccumulatedFrameTime * 40f;
                var intensity = (float) Math.Pow(ChargingIntensity, 1);
                var offset = new Vector3((float) Math.Cos(time), 0, (float) Math.Sin(time)) * intensity;
                Owner.Model.LeftShoulderJoint.TransformationMatrix = Matrix4.CreateTranslation(offset * .05f);
                Owner.Model.LeftWeaponJoint.TransformationMatrix = Matrix4.CreateTranslation(offset * .075f);
                Owner.Model.LeftElbowJoint.TransformationMatrix = Matrix4.CreateTranslation(offset * .1f);
                
                Owner.Model.RightShoulderJoint.TransformationMatrix = Owner.Model.LeftShoulderJoint.TransformationMatrix;
                Owner.Model.RightElbowJoint.TransformationMatrix = Owner.Model.LeftElbowJoint.TransformationMatrix;
                Owner.Model.RightWeaponJoint.TransformationMatrix = Owner.Model.LeftWeaponJoint.TransformationMatrix;
                Mesh.BaseTint += new Vector4(1, 0, 0, 1) * intensity;
            }
            else
            {
                Owner.Model.LeftShoulderJoint.TransformationMatrix = Matrix4.Identity;
                Owner.Model.RightShoulderJoint.TransformationMatrix = Matrix4.Identity;
                Owner.Model.LeftElbowJoint.TransformationMatrix = Matrix4.Identity;
                Owner.Model.RightElbowJoint.TransformationMatrix = Matrix4.Identity;
                Owner.Model.LeftWeaponJoint.TransformationMatrix = Matrix4.Identity;
                Owner.Model.RightWeaponJoint.TransformationMatrix = Matrix4.Identity;
            }
        }

        public virtual Vector3 WeaponTip =>
            Vector3.TransformPosition((-Vector3.UnitY * 1.5f + Vector3.UnitX * 3f) * 2F, Owner.Model.LeftWeaponMatrix);

        public EffectDescriber Describer
        {
            get => _describer;
            set
            {
                _describer = value;
                _effectApplied = false;
            }
        }

        public bool InAttackStance
        {
            get
            {
                if (_onAttackStance) return true;

                if (Owner == null)
                    return false;

                return Owner.Model.AnimationBlending == AttackStanceAnimation || Owner.Model.AnimationBlending == ChargeStanceAnimation;
            }
            set
            {
                if(value == true) _passedTimeInAttackStance = 0;
                if (_onAttackStance == value) return;
                if (value)
                {
                    RoutineManager.StartRoutine(WasAttackingCoroutine);
                }
                else
                {
                    _onAttackStance = false;
                }
            }
        }

        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Enabled = value;
                }
                _enabled = value;
            }
        }

        public bool ApplyFog
        {
            get => _applyFog;
            set
            {
                if (_applyFog == value) return;
                _applyFog = value;
                GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].ApplyFog = _applyFog;
                }
            }
        }

        public bool Pause
        {
            get => _pause;
            set
            {
                if (_pause == value) return;
                _pause = value;
                GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Pause = _pause;
                }
            }
        }

        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale == value) return;
                _scale = value;
                GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Scale = _scale;
                }
            }
        }

        public float Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha == value) return;

                GatherMembers();
                _alpha = value;
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Alpha = _alpha;
                }
            }
        }

        public bool Outline
        {
            get => _outline;
            set
            {
                if (_outline == value) return;

                GatherMembers();
                _outline = value;
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Outline = _outline;
                }
            }
        }
        
        public Vector4 OutlineColor
        {
            get => _outlineColor;
            set
            {
                if (_outlineColor == value) return;

                GatherMembers();
                _outlineColor = value;
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].OutlineColor = _outlineColor;
                }
            }
        }
        
        public float AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                if (_animationSpeed == value) return;

                GatherMembers();
                _animationSpeed = value;
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].AnimationSpeed = _animationSpeed;
                }
            }
        }

        public Vector3 Position
        {
            get => MainMesh.Position;
            set => MainMesh.Position = value;
        }

        public Vector3 LocalRotation
        {
            get => MainMesh.LocalRotation;
            set => MainMesh.LocalRotation = value;
        }

        public bool Sheathed => !LockWeapon && !PrimaryAttack && !SecondaryAttack &&
                                Owner != null && !Owner.WasAttacking &&
                                !InAttackStance;

        public static Weapon Empty => new Hands();

        public void GatherMembers(bool Force = false)
        {
            if (Meshes == null || Force) GatherMeshes();
            if (_animations == null || Force) GatherAnimations();
        }

        private void GatherMeshes()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var meshList = new List<ObjectMesh>();
            var fields = GetType().GetFields(flags);
            var propierties = GetType().GetProperties(flags);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(ObjectMesh)) meshList.Add(field.GetValue(this) as ObjectMesh);
                if (field.FieldType == typeof(Weapon)) meshList.Add((field.GetValue(this) as Weapon)?.MainMesh);
            }
            foreach (var property in propierties)
            {
                if (property.PropertyType == typeof(ObjectMesh)) meshList.Add(property.GetValue(this, null) as ObjectMesh);
                if (property.PropertyType == typeof(Weapon)) meshList.Add((property.GetValue(this, null) as Weapon)?.MainMesh);
            }
            Meshes = meshList.ToArray();
        }

        private void GatherAnimations()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var animList = new List<Animation>();
            foreach (var field in GetType().GetFields(flags))
            {
                if (field.FieldType == typeof(Animation))
                {
                    var animation = field.GetValue(this) as Animation;
                    if (!animation.Loop) animList.Add(animation);
                }
                if (field.FieldType != typeof(Animation[])) continue;
                var array = field.GetValue(this) as Animation[];
                if (array == null) continue;
                for (var i = 0; i < array.Length; i++)
                {
                    if (!array[i].Loop) animList.Add(array[i]);
                }
            }
        }

        public void StartWasAttackingCoroutine()
        {
            if (Owner != null)
            {
                Owner.WasAttacking = true;
                RoutineManager.StartRoutine(WasAttackingCoroutine);
            }
        }

        private IEnumerator DisableWasAttacking()
        {
            float time = 0;
            while (time < 0.20f)
            {
                time += Time.DeltaTime;

                yield return null;
            }
            if (Owner != null)
                Owner.WasAttacking = false;
        }

        private IEnumerator WasAttackingCoroutine()
        {
            _onAttackStance = true;
            if (WeaponCoroutineExists)
                yield break;
            else
                WeaponCoroutineExists = true;
            RoutineManager.StartRoutine(DisableWasAttacking);
            _passedTimeInAttackStance = 0;
            while (_passedTimeInAttackStance < 5f && _onAttackStance)
            {
                if (Owner.IsAttacking)
                {
                    if (Owner.IsAttacking)
                        Owner.WasAttacking = false;
                    WeaponCoroutineExists = false;
                    yield break;
                }

                Owner.Model.DefaultBlending = IsCharging ? ChargeStanceAnimation : AttackStanceAnimation;
                _passedTimeInAttackStance += Time.DeltaTime;
                yield return null;
            }
            if (Owner != null && !Owner.IsAttacking)
            {
                Owner.Model.DefaultBlending = null;
            }
            _onAttackStance = false;
            WeaponCoroutineExists = false;
        }

        public virtual void Dispose()
        {
            GatherMembers(true);
            foreach (ObjectMesh mesh in Meshes) mesh.Dispose();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var field in GetType().GetFields(flags))
            {
                if (field.FieldType != typeof(ObjectMesh[])) continue;

                var meshArray = field.GetValue(this) as ObjectMesh[];
                if (meshArray == Meshes) continue;
                foreach (ObjectMesh meshItem in meshArray) meshItem.Dispose();
            }
            Disposed = true;
        }
    }

    public enum AttackEventType
    {
        Start,
        Mid,
        End
    }
}