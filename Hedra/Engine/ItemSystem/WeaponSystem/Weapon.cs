/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 06/05/2016
 * Time: 09:17 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

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
        private static Weapon _empty;
        public EntityMesh Mesh;
        public Animation AttackStanceAnimation { get; set; }
        public virtual Vector3 SheathedPosition => new Vector3(-.6f, 0.5f, -0.8f);
        public virtual Vector3 SheathedRotation => new Vector3(-5, 90, -135);
        public VertexData MeshData;
        public bool LockWeapon = false;
        protected HumanModel Model;
        public bool WeaponCoroutineExists;
        private bool _onAttackStance;
        public bool Disposed { get; private set; }
        public bool ContinousAttack { get; set; }
        protected TrailRenderer _trail;
        protected Animation[] _animations;
        protected Animation[] SecondaryAnimations;
        protected Animation[] PrimaryAnimations;
        protected bool SecondaryAttack;
        protected bool PrimaryAttack;
        protected int PrimaryAnimationsIndex;
        protected int SecondaryAnimationsIndex;
        protected bool Orientate { get; set; } = true;
        protected SoundType SoundType { get; set; } = SoundType.SlashSound;
        protected bool ShouldPlaySound { get; set; } = true;
        public abstract bool IsMelee { get; protected set; }

        public bool InAttackStance
        {
            get
            {
                if (_onAttackStance) return true;

                if (Model == null)
                    return false;

                return Model.Model.Animator.BlendingAnimation == AttackStanceAnimation;
            }
            set { _onAttackStance = value; }
        }

        public bool Sheathed => !LockWeapon && !PrimaryAttack && !SecondaryAttack && (Model != null && !Model.Human.WasAttacking) &&
                                !InAttackStance;

        protected void SetToDefault(EntityMesh Mesh)
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

        protected void SetToChest(EntityMesh Mesh)
        {
            this.SetToDefault(Mesh);
            Mesh.Position = Model.Position;
            Mesh.TargetPosition = Model.ChestPosition - this.Mesh.Position + (this.SheathedPosition - Vector3.UnitZ * .5f) * this.Scale;
            Mesh.AnimationPosition = Model.ChestPosition - this.Mesh.Position + (this.SheathedPosition - Vector3.UnitZ * .5f) * this.Scale;
            Mesh.RotationPoint = -(Model.ChestPosition - this.Mesh.Position);
            Mesh.Rotation = Model.Rotation;
            Mesh.LocalRotation = this.SheathedRotation;
        }

        protected void SetToMainHand(EntityMesh Mesh)
        {
            Matrix4 Mat4 = Model.LeftHandMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Model.Position + Model.LeftHandPosition);

            Mesh.TransformationMatrix = Mat4;
            Mesh.Position = Model.Position;
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

        public virtual void Attack1(HumanModel Model)
        {
            if (!this.MeetsRequirements()) return;

            PrimaryAnimationsIndex++;

            if (PrimaryAnimationsIndex == PrimaryAnimations.Length)
                PrimaryAnimationsIndex = 0;

            this.BasePrimaryAttack(Model);
        }

        protected void BasePrimaryAttack(HumanModel Model)
        {
            this.BaseAttack(Model);
            Model.Model.BlendAnimation(PrimaryAnimations[this.ParsePrimaryIndex(PrimaryAnimationsIndex)]);
        }

        public virtual void Attack2(HumanModel Model)
        {
            if (!this.MeetsRequirements()) return;

            SecondaryAnimationsIndex++;

            if (SecondaryAnimationsIndex == SecondaryAnimations.Length)
                SecondaryAnimationsIndex = 0;

            this.BaseSecondaryAttack(Model);
        }

        protected void BaseSecondaryAttack(HumanModel Model)
        {
            this.BaseAttack(Model);
            Model.Model.BlendAnimation(SecondaryAnimations[this.ParseSecondaryIndex(SecondaryAnimationsIndex)]);
        }

        protected bool MeetsRequirements()
        {
            return Model != null && !(Model.Human.IsAttacking || Model.Human.Knocked || SecondaryAttack || PrimaryAttack);
        }

        public void PlaySound()
        {
            SoundManager.PlaySoundWithVariation(SoundType, Model.Human.Position);
        }

        protected void BaseAttack(HumanModel Model)
        {
            this.Init(Model);
            Model.Model.Animator.StopBlend();

            if(ShouldPlaySound && !IsMelee)
                SoundManager.PlaySoundWithVariation(SoundType, Model.Human.Position);

            if (Orientate && Model.Human is LocalPlayer)
                Model.Human.Movement.OrientatePlayer(Model.Human as LocalPlayer);
        }

        protected Weapon(VertexData MeshData)
        {
            if (MeshData != null)
            {
                VertexData baseMesh = MeshData.Clone();
                baseMesh.Scale(Vector3.One * 1.75f);
                this.Mesh = EntityMesh.FromVertexData(baseMesh);
            }
            this.MeshData = MeshData;
        }


        public virtual void Update(HumanModel Model)
        {
            this.Init();
            this.Init(Model);

            if (_trail == null)
            {
                this._trail = new TrailRenderer(
                    () => this.WeaponTip,
                    Vector4.One);
            }
            this._trail.Update();

            var attacking = false;
            for (int i = 0; i < _animations.Length; i++)
            {
                if (_animations[i] == Model.Model.Animator.AnimationPlaying ||
                    _animations[i] == Model.Model.Animator.BlendingAnimation)
                {
                    attacking = true;
                    break;
                }
            }
            if (!attacking && Model.Human.IsAttacking)
            {
                Model.Human.WasAttacking = true;
                CoroutineManager.StartCoroutine(this.WasAttackingCoroutine);
            }


            var primaryAttack = false;
            for (int i = 0; i < PrimaryAnimations.Length; i++)
            {
                if (PrimaryAnimations[i] == Model.Model.Animator.AnimationPlaying ||
                    PrimaryAnimations[i] == Model.Model.Animator.BlendingAnimation)
                {
                    primaryAttack = true;
                    break;
                }
            }
            PrimaryAttack = primaryAttack;

            var secondaryAttack = false;
            for (int i = 0; i < SecondaryAnimations.Length; i++)
            {
                if (SecondaryAnimations[i] == Model.Model.Animator.AnimationPlaying ||
                    SecondaryAnimations[i] == Model.Model.Animator.BlendingAnimation)
                {
                    secondaryAttack = true;
                    break;
                }
            }
            SecondaryAttack = secondaryAttack;



            Model.Human.IsAttacking = attacking;


            if (Sheathed)
                this.SetToChest(Mesh);


            if (InAttackStance || Model.Human.WasAttacking)
                this.SetToMainHand(Mesh);


            if (PrimaryAttack)
                this.SetToMainHand(Mesh);

            if (SecondaryAttack)
                this.SetToMainHand(Mesh);

            if (PrimaryAttack || SecondaryAttack)
            {
                if (Orientate && this.Model.Human is LocalPlayer)
                    this.Model.Human.Movement.OrientatePlayer(Model.Human as LocalPlayer);
            }
        }

        public virtual Vector3 WeaponTip =>
            Vector3.TransformPosition(-Vector3.UnitY * 1.5f + Vector3.UnitX * 3f, Model.LeftHandMatrix);


        private bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                this.Init();
                for (int i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Enabled = value;
                }
                _enabled = value;
            }
        }

        private Vector3 _scale = Vector3.One;

        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value) return;
                _scale = value;
                this.Init();
                for (int i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Scale = _scale;
                }
            }
        }

        private float _alpha = 1f;

        public float Alpha
        {
            get { return _alpha; }
            set
            {
                if(_alpha == value) return;

                this.Init();
                _alpha = value;
                for (int i = 0; i < Meshes.Length; i++)
                {
                    Meshes[i].Alpha = _alpha;
                }
            }
        }

        public EntityMesh[] Meshes;

        public void Init(bool Force = false)
        {
            if (Meshes == null || Force)
            {
                List<EntityMesh> MeshList = new List<EntityMesh>();
                BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (FieldInfo Field in this.GetType().GetFields(Flags))
                {
                    if (Field.FieldType == typeof(EntityMesh))
                        MeshList.Add(Field.GetValue(this) as EntityMesh);
                    if (Field.FieldType == typeof(Weapon))
                        MeshList.Add((Field.GetValue(this) as Weapon).Mesh);
                }

                Meshes = MeshList.ToArray();
            }

            if (_animations == null || Force)
            {
                var animList = new List<Animation>();
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (FieldInfo Field in this.GetType().GetFields(flags))
                {
                    if (Field.FieldType == typeof(Animation))
                    {
                        var field = Field.GetValue(this) as Animation;
                        if (!field.Loop)
                            animList.Add(field);
                    }
                    if (Field.FieldType == typeof(Animation[]))
                    {
                        var array = Field.GetValue(this) as Animation[];
                        if (array != null)
                        {
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (!array[i].Loop)
                                    animList.Add(array[i]);
                            }
                        }
                    }
                }
                _animations = animList.ToArray();
            }
        }

        public void StartWasAttackingCoroutine()
        {
            if (Model != null)
            {
                Model.Human.WasAttacking = true;
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
            if (Model != null)
                Model.Human.WasAttacking = false;
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
                if (Model != null && Model.Human.IsAttacking)
                {
                    if (Model.Human.IsAttacking)
                        Model.Human.WasAttacking = false;
                    WeaponCoroutineExists = false;
                    yield break;
                }

                Model?.Model.BlendAnimation(AttackStanceAnimation);
                passedTime += Time.ScaledFrameTimeSeconds;
                yield return null;
            }
            if (Model != null && !Model.Human.IsAttacking)
            {
                if (Model.Model.Animator.BlendingAnimation == AttackStanceAnimation)
                    Model.Model.Animator.ExitBlend();
            }
            _onAttackStance = false;
            WeaponCoroutineExists = false;
        }

        public void Dispose()
        {
            this.Init(true);
            for (int i = 0; i < Meshes.Length; i++)
                Meshes[i].Dispose();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo Field in this.GetType().GetFields(flags))
            {
                if (Field.FieldType != typeof(EntityMesh[])) continue;

                var MeshArray = Field.GetValue(this) as EntityMesh[];
                if(MeshArray == this.Meshes) continue;
                for (int j = 0; j < MeshArray.Length; j++)
                    MeshArray[j].Dispose();
            }
            Disposed = true;
        }

        private bool _inited;

        public void Init(HumanModel Model)
        {
            if (_inited) return;
            _inited = true;
            this.Model = Model;
        }

        public bool SlowDown { get; set; }

        public Vector3 Position
        {
            get { return Mesh.Position; }
            set { Mesh.Position = value; }
        }

        public Vector3 Rotation
        {
            get { return Mesh.Rotation; }
            set { Mesh.Rotation = value; }
        }

        public static Weapon Empty => _empty ?? (_empty = new Hands());
    }
}