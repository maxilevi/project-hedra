/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 23/04/2016
 * Time: 02:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using System.Reflection;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of PlayerModel.
    /// </summary>
    public class HumanoidModel : AnimatedUpdatableModel, IAudible, IDisposeAnimation
    {
        private const float DefaultScale = 0.75f;
        private float _alpha = 1f;
        private Animation _animationPlaying;
        private AnimatedCollider _collider;
        private Vector3 _defaultChestPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 _defaultHeadPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 _defaultLeftFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 _defaultLeftWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 _defaultRightFootPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Vector3 _defaultRightWeaponPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private StaticModel _food;
        private Timer _foodTimer;
        private bool _hasLamp;
        private bool _isDisposeAnimationPlaying;
        private bool _isEatingWhileSitting;
        private ObjectMesh _lampModel;
        private float _lastAnimationTime = -1;
        private AreaSound _modelSound;
        private Vector3 _previousPosition;
        private Vector3 _rotation;
        private Quaternion _rotationQuaternion;


        public HumanoidModel(IHumanoid Human, HumanoidModelTemplate Template) : base(Human)
        {
            Load(Human, Template);
        }

        public HumanoidModel(IHumanoid Human, HumanType Type) : base(Human)
        {
            Load(Human, HumanoidLoader.HumanoidTemplater[Type].RandomModel);
        }

        public HumanoidModel(IHumanoid Human) : base(Human)
        {
            Load(Human, Human.Class.ModelTemplate);
        }

        public IHumanoid Human { get; private set; }
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
        public HumanoidModelTemplate Template { get; private set; }
        public HumanoidModelAnimationState StateHandler { get; private set; }
        public Vector3 RidingOffset { get; set; }

        public override CollisionShape HorizontalBroadphaseCollider => _collider.HorizontalBroadphase;
        public override bool IsWalking => StateHandler.IsWalking;
        public override Vector4 Tint { get; set; }
        public override Vector4 BaseTint { get; set; }
        protected override string ModelPath { get; set; }

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

        public Matrix4x4 HeadMatrix => Model.MatrixFromJoint(HeadJoint);

        public Matrix4x4 ChestMatrix => Model.MatrixFromJoint(ChestJoint);

        public Matrix4x4 LeftWeaponMatrix => Model.MatrixFromJoint(LeftWeaponJoint);

        public Matrix4x4 RightWeaponMatrix => Model.MatrixFromJoint(RightWeaponJoint);

        public Matrix4x4 LeftFootMatrix => Model.MatrixFromJoint(LeftFootJoint);

        public Matrix4x4 RightFootMatrix => Model.MatrixFromJoint(RightFootJoint);

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
                if (_defaultLeftWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    _defaultLeftWeaponPosition = Model.JointDefaultPosition(LeftWeaponJoint);
                return Model.TransformFromJoint(_defaultLeftWeaponPosition, LeftWeaponJoint);
            }
        }

        public Vector3 RightFootPosition
        {
            get
            {
                if (_defaultRightFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    _defaultRightFootPosition = Model.JointDefaultPosition(RightFootJoint);
                return Model.TransformFromJoint(_defaultRightFootPosition, RightFootJoint);
            }
        }

        public Vector3 LeftFootPosition
        {
            get
            {
                if (_defaultLeftFootPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
                    _defaultLeftFootPosition = Model.JointDefaultPosition(LeftFootJoint);
                return Model.TransformFromJoint(_defaultLeftFootPosition, LeftFootJoint);
            }
        }

        public Vector3 RightWeaponPosition
        {
            get
            {
                if (_defaultRightWeaponPosition == new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
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

        public override Vector3 Position { get; set; }

        public Matrix4x4 TransformationMatrix { get; set; } = Matrix4x4.Identity;

        public Matrix4x4 TiltMatrix { get; set; } = Matrix4x4.Identity;

        public override Vector3 LocalRotation { get; set; }

        public Animation DefaultBlending
        {
            get => StateHandler.DefaultBlending;
            set => StateHandler.DefaultBlending = value;
        }

        public void StopSound()
        {
            _modelSound.Stop();
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

        private void Load(IHumanoid Humanoid, HumanoidModelTemplate Template)
        {
            this.Template = Template;
            Human = Humanoid;
            Scale = Vector3.One * Template.Scale;
            Tint = Vector4.One;
            IsUndead = Template.IsUndead;
            ModelPath = Template.Path;


            Model = AnimationModelLoader.LoadEntity(ModelPath, true);
            Model.IgnoreBaseModel = Template.IgnoreBaseModel;
            /* If we are ignoring the base model then we are using body parts */
            UsesBodyParts = Model.IgnoreBaseModel;
            StateHandler = BuildAnimationHandler(Humanoid, Template);

            Model.Scale = Vector3.One * DefaultScale * Template.Scale;
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

            _collider = new AnimatedCollider(Template.Path, Model);
            BaseBroadphaseBox = AssetManager.LoadHitbox(Template.Path) * Model.Scale;
            Dimensions = AssetManager.LoadDimensions(Template.Path) * Model.Scale;
            _modelSound = new AreaSound(SoundType.HumanRun, Vector3.Zero, 48f);
            _modelSound.Volume = 0.5f;
            _foodTimer = new Timer(1)
            {
                AutoReset = false
            };
            _food = new StaticModel(VertexData.Empty)
            {
                Scale = Vector3.One * 1.5f
            };
        }

        public void SetLamp(bool Active)
        {
            if (_hasLamp == Active) return;
            _hasLamp = Active;

            if (_lampModel == null)
            {
                var lampData = AssetManager.PLYLoader("Assets/Items/Handlamp.ply", new Vector3(1.5f, 1.5f, 1.5f),
                    Vector3.Zero, Vector3.Zero);
                _lampModel = ObjectMesh.FromVertexData(lampData);
                RegisterModel(_lampModel);
            }

            _lampModel.Enabled = Active;
        }

        public void EatFood(Item Food, Action<Item> OnEatingEnd)
        {
            _foodTimer.AlertTime = Food.GetAttribute<float>(CommonAttributes.EatTime);
            _foodTimer.Reset();
            _isEatingWhileSitting = Food.HasAttribute(CommonAttributes.EatSitting) &&
                                    Food.GetAttribute<bool>(CommonAttributes.EatSitting);
            _food.SetModel(Food.Model);
            DoEat(Food, OnEatingEnd);
        }

        private void StopEating()
        {
            _food.Enabled = false;
            Human.IsEating = false;
            Model.ResetBlending();
        }

        private void DoEat(Item Food, Action<Item> OnEatingEnd)
        {
            Human.IsEating = true;
            var health = Food.GetAttribute<float>(CommonAttributes.Saturation);
            var healthPerSecond = health / _foodTimer.AlertTime;
            TaskScheduler.While(
                () => Human.IsEating && !Human.IsDead,
                () => Human.Health += healthPerSecond * Time.DeltaTime * 2);
            TaskScheduler.When(
                () => !Human.IsEating,
                () => OnEatingEnd(Food)
            );
            Human.WasAttacking = false;
            Human.IsAttacking = false;
            SoundPlayer.PlaySound(SoundType.FoodEat, Position);
        }

        private void HandleState()
        {
            StateHandler.SelectAnimation(out var currentAnimation, out var blendingAnimation);

            if (currentAnimation != null && Model.AnimationPlaying != currentAnimation
                                         && (Model.AnimationPlaying != _animationPlaying || _animationPlaying == null))
                Model.PlayAnimation(currentAnimation);
            if (blendingAnimation != null && Model.AnimationBlending != blendingAnimation)
                if (!(blendingAnimation == DefaultBlending && Model.AnimationBlending != null))
                    Model.BlendAnimation(blendingAnimation);
        }

        public void Play(Animation Animation)
        {
            PlayAnimation(Animation);
            BlendAnimation(Animation);
        }

        public void PlayAnimation(Animation Animation)
        {
            _animationPlaying = Animation;
            Model.PlayAnimation(_animationPlaying);
        }

        public void BlendAnimation(Animation Animation)
        {
            Model.BlendAnimation(Animation);
        }

        public void Greet(Action Callback)
        {
            StateHandler.Greet(Callback);
        }

        public void Reset()
        {
            Model.Reset();
        }

        private void HandleEatingEffects()
        {
            var mat4 = LeftWeaponMatrix.ClearTranslation() *
                       Matrix4x4.CreateTranslation(-Position + (LeftWeaponPosition + RightWeaponPosition) / 2f);

            _food.TransformationMatrix = mat4;
            _food.Position = Position;
            _food.TargetRotation = Vector3.Zero;
            _food.LocalRotationPoint = Vector3.Zero;
            _food.LocalRotation = Vector3.Zero;
            _food.Rotation = new Vector3(90, 0, 0);
            _food.LocalPosition = Vector3.Zero;
            _food.BeforeRotation = Vector3.Zero;
            _food.Enabled = true;
        }

        private void HandleRollEffects()
        {
            if ((_previousPosition - Human.Position).LengthFast() > 1 && Human.IsGrounded)
            {
                World.Particles.VariateUniformly = true;
                World.Particles.Color = Vector4.One;
                World.Particles.Position = Human.Position - Vector3.UnitY;
                World.Particles.Scale = Vector3.One * .5f;
                World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
                World.Particles.Direction = (-Human.Orientation + Vector3.UnitY * 2.75f) * .15f;
                World.Particles.ParticleLifetime = 1;
                World.Particles.GravityEffect = .1f;
                World.Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);

                for (var i = 0; i < 1; i++) World.Particles.Emit();
            }

            _previousPosition = Human.Position;
        }

        public override void Update()
        {
            base.Update();
            StateHandler.Update();
            _modelSound.Pitch = Human.Speed / PitchSpeed;
            var rotation = TargetRotation * Mathf.Radian;
            _rotationQuaternion = Quaternion.Slerp(_rotationQuaternion, QuaternionMath.FromEuler(rotation),
                Time.IndependentDeltaTime * 8f);
            var a = _rotationQuaternion.ToEuler();
            if (a.IsInvalid())
            {
                int b = 0;
            }
            Model.LocalRotation = a;
            LocalRotation = Model.LocalRotation;
            HandleTransformationMatrix();
            HandleState();
            if (_isEatingWhileSitting && !Human.IsSitting && Human.IsEating) StopEating();
            if (_foodTimer.Tick() && Human.IsEating) StopEating();
            Human.HandLamp.Update();
            if (!Disposed)
            {
                _modelSound.Type = SoundType.HumanRun; /*Human.IsSleeping 
                    ? SoundType.HumanSleep
                    : Human.Physics.IsOverAShape
                        ? SoundType.HumanRunWood
                        : SoundType.HumanRun;*/
                _modelSound.Position = Position;
                _modelSound.Update(IsWalking && !Human.IsJumping && !Human.IsSwimming && Human.IsGrounded ||
                                   Human.IsSleeping);
            }

            if (_hasLamp)
            {
                _lampModel.Position = LeftWeaponPosition;
                _lampModel.LocalRotation = LocalRotation;
                _lampModel.LocalRotationPoint = Vector3.Zero;
            }

            if (Human.IsRolling) HandleRollEffects();
            if (Human.IsEating)
            {
                HandleEatingEffects();
                _foodTimer.Tick();
            }
        }

        public override void BaseUpdate()
        {
            base.BaseUpdate();
            Model.Position = Mathf.Lerp(Model.Position, Position + RidingOffset, Time.IndependentDeltaTime * 24f);
            Model.Update();
        }

        public void SetValues(HumanoidModel HumanModel)
        {
            Model.SetValues(HumanModel.Model);
        }

        private void HandleTransformationMatrix()
        {
            var ridingOffsetMatrix = Matrix4x4.CreateTranslation(RidingOffset);
            var ridingOffsetMatrixInverted = Matrix4x4.CreateTranslation(-RidingOffset);
            var tiltTransformation =
                Matrix4x4.CreateRotationY(-Model.LocalRotation.Y * Mathf.Radian) *
                ridingOffsetMatrix *
                TiltMatrix *
                ridingOffsetMatrixInverted *
                Matrix4x4.CreateRotationY(Model.LocalRotation.Y * Mathf.Radian);
            Model.TransformationMatrix = tiltTransformation * TransformationMatrix;
        }

        public void RegisterEquipment(IModel Equipment)
        {
            Equipment.Enabled = Enabled;
            Equipment.Scale *= Model.Scale;
            Equipment.Alpha = Model.Alpha;
            RegisterModel(Equipment);
        }

        public void UnregisterEquipment(IModel Equipment)
        {
            UnregisterModel(Equipment);
        }

        public Vector3 TransformFromJoint(Vector3 Point, Joint Joint)
        {
            return Model.TransformFromJoint(Point, Joint);
        }

        public Vector3 JointDefaultPosition(Joint Joint)
        {
            return Model.JointDefaultPosition(Joint);
        }

        protected virtual HumanoidModelAnimationState BuildAnimationHandler(IHumanoid Humanoid,
            HumanoidModelTemplate Template)
        {
            return new HumanoidModelAnimationState(Humanoid, this, Template);
        }

        public bool HasModel(ModelData NewModel)
        {
            return Model.HasModel(NewModel);
        }

        private static ModelData LoadAndPaint(IHumanoid Owner, string Path)
        {
            var model = ModelLoader.Load(Path);
            PaintModelWithCustomization(Owner, model);
            return model;
        }

        public static void PaintModelWithCustomization(IHumanoid Owner, ModelData Model)
        {
            Model.Paint(AssetManager.ColorCode0.Xyz(), Owner.Customization.SkinColor.Xyz());
            Model.Paint(AssetManager.ColorCode1.Xyz(), Owner.Customization.FirstHairColor.Xyz());
            Model.Paint(AssetManager.ColorCode2.Xyz(), Owner.Customization.SecondHairColor.Xyz());
        }

        public static ModelData LoadHead(IHumanoid Owner)
        {
            return LoadAndPaint(Owner,
                Owner.Customization.Gender == HumanGender.Male
                    ? Owner.Class.HeadModelTemplate.Path
                    : Owner.Class.FemaleHeadModelTemplate.Path);
        }

        public static ModelData LoadChest(IHumanoid Owner)
        {
            return LoadAndPaint(Owner,
                Owner.Customization.Gender == HumanGender.Male
                    ? Owner.Class.ChestModelTemplate.Path
                    : Owner.Class.FemaleChestModelTemplate.Path);
        }

        public static ModelData LoadLegs(IHumanoid Owner)
        {
            return LoadAndPaint(Owner,
                Owner.Customization.Gender == HumanGender.Male
                    ? Owner.Class.LegsModelTemplate.Path
                    : Owner.Class.FemaleLegsModelTemplate.Path);
        }

        public static ModelData LoadFeet(IHumanoid Owner)
        {
            return LoadAndPaint(Owner,
                Owner.Customization.Gender == HumanGender.Male
                    ? Owner.Class.FeetModelTemplate.Path
                    : Owner.Class.FemaleFeetModelTemplate.Path);
        }

        public bool UpdateWhenOutOfView
        {
            get => Model.UpdateWhenOutOfView;
            set => Model.UpdateWhenOutOfView = value;
        }
        
        public override void Dispose()
        {
            _food.Dispose();
            _collider.Dispose();
            Model.Dispose();
            _lampModel?.Dispose();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var field in GetType().GetFields(flags))
                if (field.GetType() == typeof(Animation))
                    (field.GetValue(this) as Animation)?.Dispose();
            base.Dispose();
        }
    }

    public enum HumanType
    {
        Warrior,
        Archer,
        Beasthunter,
        Rogue,
        Skeleton,
        Merchant,
        Blacksmith,
        Mandragora,
        TravellingMerchant,
        Gnoll,
        WerewolfMorph,
        EntMorph,
        Witch,
        Bard,
        Scholar,
        Farmer,
        Mage,
        Innkeeper,
        Clothier,
        VillagerGhost,
        Mason,
        Fisherman,
        GnollWarrior,
        GnollMage,
        GnollRogue,
        GnollArcher,
        BeasthunterSpirit,
        GreenVillager
    }
}