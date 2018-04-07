﻿/*
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
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    /// <summary>
    /// Description of Weapon.
    /// </summary>
    public abstract class Weapon
    {
        public ObjectMesh MainMesh { get; }
        public ObjectMesh[] Meshes { get; private set; }
        public VertexData MeshData { get; }
        public Animation AttackStanceAnimation { get; set; }
        public virtual Vector3 SheathedPosition => new Vector3(-.6f, 0.5f, -0.8f);
        public virtual Vector3 SheathedRotation => new Vector3(-5, 90, -135);
        public abstract bool IsMelee { get; protected set; }
        public bool LockWeapon { get; set; } = false;
        public bool WeaponCoroutineExists;
        public bool Disposed { get; private set; }
        public bool ContinousAttack { get; set; }
        public bool SlowDown { get; set; }
        public Vector4 BaseTint { get; set; }
        public Vector4 Tint { get; set; }
        protected Humanoid Owner { get; set; }
        protected TrailRenderer Trail { get; set; }
        protected Animation[] Animations { get; set; }
        protected Animation[] SecondaryAnimations { get; set; }
        protected Animation[] PrimaryAnimations { get; set; }
        protected bool SecondaryAttack { get; set; }
        protected bool PrimaryAttack { get; set; }
        protected int PrimaryAnimationsIndex { get; set; }
        protected int SecondaryAnimationsIndex { get; set; }
        protected bool Orientate { get; set; } = true;
        protected SoundType SoundType { get; set; } = SoundType.SlashSound;
        protected bool ShouldPlaySound { get; set; } = true;

        private float[] _animationSpeeds;
        private EffectDescriber _describer;
        private bool _onAttackStance;
        private Vector3 _scale = Vector3.One;
        private float _alpha = 1f;
        private bool _effectApplied;

        protected Weapon(VertexData MeshData)
        {
            if (MeshData != null)
            {
                VertexData baseMesh = MeshData.Clone();
                baseMesh.Scale(Vector3.One * 1.75f);
                this.MainMesh = ObjectMesh.FromVertexData(baseMesh);
            }
            else
            {
                this.MainMesh = new ObjectMesh(Vector3.Zero);
            }
            this.MeshData = MeshData;
        }

        protected void SetToDefault(ObjectMesh Mesh)
        {
            Mesh.Position = Vector3.Zero;
            Mesh.TargetPosition = Vector3.Zero;
            Mesh.AnimationPosition = Vector3.Zero;
            Mesh.RotationPoint = Vector3.Zero;
            Mesh.Rotation = Vector3.Zero;
            Mesh.LocalRotation = Vector3.Zero;
            Mesh.LocalRotationPoint = Vector3.Zero;
            Mesh.LocalPosition = Vector3.Zero;
            Mesh.BeforeLocalRotation = Vector3.Zero;
            Mesh.TransformationMatrix = Matrix4.Identity;
            Mesh.TargetRotation = Vector3.Zero;
        }

        protected void SetToChest(ObjectMesh Mesh)
        {
            this.SetToDefault(Mesh);
            Mesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() *
                                        Matrix4.CreateTranslation(
                                            -Owner.Position + Owner.Model.ChestPosition + Vector3.UnitY * 1f);
            Mesh.Position = Owner.Position;
            Mesh.LocalRotation = this.SheathedRotation;
            Mesh.BeforeLocalRotation =
                (this.SheathedPosition + Vector3.UnitX * 2.25f + Vector3.UnitZ * 2.5f) * this.Scale;
        }

        protected void SetToMainHand(ObjectMesh Mesh)
        {
            Matrix4 Mat4 = Owner.Model.LeftHandMatrix.ClearTranslation() *
                           Matrix4.CreateTranslation(-Owner.Position + Owner.Model.LeftHandPosition);

            Mesh.TransformationMatrix = Mat4;
            Mesh.Position = Owner.Position;
            Mesh.TargetPosition = Vector3.Zero;
            Mesh.TargetRotation = new Vector3(180, 0, 0);
            Mesh.AnimationPosition = Vector3.Zero;
            Mesh.RotationPoint = Vector3.Zero;
            Mesh.Rotation = Vector3.Zero;
            Mesh.LocalRotation = Vector3.Zero;
            Mesh.LocalPosition = Vector3.Zero;
            Mesh.BeforeLocalRotation = Vector3.UnitY * -0.7f;
        }

        public virtual int ParsePrimaryIndex(int Index)
        {
            return Index;
        }

        public virtual int ParseSecondaryIndex(int Index)
        {
            return Index;
        }

        public virtual void Attack1(Humanoid Human)
        {
            if (!this.MeetsRequirements()) return;

            PrimaryAnimationsIndex++;

            if (PrimaryAnimationsIndex == PrimaryAnimations.Length)
                PrimaryAnimationsIndex = 0;

            this.BasePrimaryAttack(Human);
        }

        protected void BasePrimaryAttack(Humanoid Human)
        {
            this.BaseAttack(Human);
            var animation = PrimaryAnimations[this.ParsePrimaryIndex(PrimaryAnimationsIndex)];
            animation.Speed = _animationSpeeds[Array.IndexOf(Animations, animation)] * Human.AttackSpeed;
            Human.Model.Model.BlendAnimation(animation);
        }

        public virtual void Attack2(Humanoid Human)
        {
            if (!this.MeetsRequirements()) return;

            SecondaryAnimationsIndex++;

            if (SecondaryAnimationsIndex == SecondaryAnimations.Length)
                SecondaryAnimationsIndex = 0;

            this.BaseSecondaryAttack(Human);
        }

        protected void BaseSecondaryAttack(Humanoid Human)
        {
            this.BaseAttack(Human);
            var animation = SecondaryAnimations[this.ParseSecondaryIndex(SecondaryAnimationsIndex)];
            animation.Speed = _animationSpeeds[Array.IndexOf(Animations, animation)] * Human.AttackSpeed;
            Human.Model.Model.BlendAnimation(animation);
        }

        protected bool MeetsRequirements()
        {
            return Owner != null &&
                   !(Owner.IsAttacking || Owner.Knocked || SecondaryAttack || PrimaryAttack);
        }

        public void PlaySound()
        {
            SoundManager.PlaySoundWithVariation(SoundType, Owner.Position);
        }

        protected void BaseAttack(Humanoid Human)
        {
            this.Owner = Human;
            Human.Model.Model.Animator.StopBlend();

            if (ShouldPlaySound && !IsMelee)
                SoundManager.PlaySoundWithVariation(SoundType, Human.Position);

            if (Orientate && Human is LocalPlayer)
                Human.Movement.OrientatePlayer(Human as LocalPlayer);
        }

        public virtual void Update(Humanoid Human)
        {
            this.GatherMembers();
            this.Owner = Human;

            if (Trail == null)
            {
                this.Trail = new TrailRenderer(
                    () => this.WeaponTip,
                    Vector4.One);
            }
            this.Trail.Update();

            var attacking = false;
            for (var i = 0; i < Animations.Length; i++)
            {
                if (Animations[i] != Owner.Model.Model.Animator.AnimationPlaying &&
                    Animations[i] != Owner.Model.Model.Animator.BlendingAnimation) continue;
                attacking = true;
                break;
            }
            if (!attacking && Owner.Model.Human.IsAttacking)
            {
                Owner.Model.Human.WasAttacking = true;
                CoroutineManager.StartCoroutine(this.WasAttackingCoroutine);
            }


            var primaryAttack = false;
            for (var i = 0; i < PrimaryAnimations.Length; i++)
            {
                if (PrimaryAnimations[i] != Owner.Model.Model.Animator.AnimationPlaying &&
                    PrimaryAnimations[i] != Owner.Model.Model.Animator.BlendingAnimation) continue;
                primaryAttack = true;
                break;
            }
            PrimaryAttack = primaryAttack;

            var secondaryAttack = false;
            for (int i = 0; i < SecondaryAnimations.Length; i++)
            {
                if (SecondaryAnimations[i] == Owner.Model.Model.Animator.AnimationPlaying ||
                    SecondaryAnimations[i] == Owner.Model.Model.Animator.BlendingAnimation)
                {
                    secondaryAttack = true;
                    break;
                }
            }
            SecondaryAttack = secondaryAttack;


            Owner.IsAttacking = attacking;


            if (Sheathed)
                this.SetToChest(MainMesh);


            if (InAttackStance || Owner.WasAttacking)
                this.SetToMainHand(MainMesh);


            if (PrimaryAttack)
                this.SetToMainHand(MainMesh);

            if (SecondaryAttack)
                this.SetToMainHand(MainMesh);

            if (PrimaryAttack || SecondaryAttack)
            {
                var player = this.Owner as LocalPlayer;
                if (Orientate && player != null) this.Owner.Movement.OrientatePlayer(player);
            }
            this.MainMesh.BaseTint = this.BaseTint;
            this.MainMesh.Tint = this.Tint;

            if (Describer != null && Describer.Type != EffectType.None)
            {
                this.MainMesh.BaseTint = Describer.EffectColor;
                if (!_effectApplied)
                {
                    this.Owner.ApplyEffectWhile(this.Describer.Type, () => Owner.Model.LeftWeapon == this);
                    _effectApplied = true;
                }
            }
        }

        public virtual Vector3 WeaponTip =>
            Vector3.TransformPosition((-Vector3.UnitY * 1.5f + Vector3.UnitX * 3f) * 2F, Owner.Model.LeftHandMatrix);

        public EffectDescriber Describer
        {
            get { return _describer; }
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

                return Owner.Model.Model.Animator.BlendingAnimation == AttackStanceAnimation;
            }
            set { _onAttackStance = value; }
        }

        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                this.GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Enabled = value;
                }
                _enabled = value;
            }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value) return;
                _scale = value;
                this.GatherMembers();
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Scale = _scale;
                }
            }
        }

        public float Alpha
        {
            get { return _alpha; }
            set
            {
                if (_alpha == value) return;

                this.GatherMembers();
                _alpha = value;
                for (var i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Alpha = _alpha;
                }
            }
        }

        public Vector3 Position
        {
            get { return MainMesh.Position; }
            set { MainMesh.Position = value; }
        }

        public Vector3 Rotation
        {
            get { return MainMesh.Rotation; }
            set { MainMesh.Rotation = value; }
        }

        public bool Sheathed => !LockWeapon && !PrimaryAttack && !SecondaryAttack &&
                                Owner != null && !Owner.WasAttacking &&
                                !InAttackStance;

        public static Weapon Empty => new Hands();

        public void GatherMembers(bool Force = false)
        {
            if (Meshes == null || Force) this.GatherMeshes();
            if (Animations == null || Force) this.GatherAnimations();
        }

        private void GatherMeshes()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var meshList = new List<ObjectMesh>();
            var fields = this.GetType().GetFields(flags);
            var propierties = this.GetType().GetProperties(flags);
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
            foreach (var field in this.GetType().GetFields(flags))
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
            foreach (var property in this.GetType().GetProperties(flags))
            {
                if (property.PropertyType == typeof(Animation))
                {
                    var animation = property.GetValue(this, null) as Animation;
                    if (!animation.Loop) animList.Add(animation);
                }
                if (property.PropertyType != typeof(Animation[])) continue;
                var array = property.GetValue(this, null) as Animation[];
                if (array == null) continue;
                for (var i = 0; i < array.Length; i++)
                {
                    if (!array[i].Loop) animList.Add(array[i]);
                }
            }
            Animations = animList.ToArray();
            _animationSpeeds = new float[Animations.Length];
            for (var i = 0; i < _animationSpeeds.Length; i++)
            {
                _animationSpeeds[i] = Animations[i].Speed;
            }
        }

        public void StartWasAttackingCoroutine()
        {
            if (Owner != null)
            {
                Owner.WasAttacking = true;
                CoroutineManager.StartCoroutine(this.WasAttackingCoroutine);
            }
        }

        private IEnumerator DisableWasAttacking()
        {
            float time = 0;
            while (time < 0.20f)
            {
                time += Time.ScaledFrameTimeSeconds;

                yield return null;
            }
            if (Owner != null)
                Owner.WasAttacking = false;
        }

        protected IEnumerator WasAttackingCoroutine()
        {
            _onAttackStance = true;
            CoroutineManager.StartCoroutine(this.DisableWasAttacking);
            if (WeaponCoroutineExists)
                yield break;
            else
                WeaponCoroutineExists = true;
            float passedTime = 0;
            while (passedTime < 4f)
            {
                if (Owner != null && Owner.IsAttacking)
                {
                    if (Owner.IsAttacking)
                        Owner.WasAttacking = false;
                    WeaponCoroutineExists = false;
                    yield break;
                }

                Owner?.Model.Model.BlendAnimation(AttackStanceAnimation);
                passedTime += Time.ScaledFrameTimeSeconds;
                yield return null;
            }
            if (Owner != null && !Owner.IsAttacking)
            {
                if (Owner.Model.Model.Animator.BlendingAnimation == AttackStanceAnimation)
                    Owner.Model.Model.Animator.ExitBlend();
            }
            _onAttackStance = false;
            WeaponCoroutineExists = false;
        }

        public void Dispose()
        {
            this.GatherMembers(true);
            foreach (ObjectMesh mesh in Meshes) mesh.Dispose();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var field in this.GetType().GetFields(flags))
            {
                if (field.FieldType != typeof(ObjectMesh[])) continue;

                var meshArray = field.GetValue(this) as ObjectMesh[];
                if (meshArray == this.Meshes) continue;
                foreach (ObjectMesh meshItem in meshArray) meshItem.Dispose();
            }
            this.Disposed = true;
        }
    }
}