using System.Globalization;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Blink : SingleAnimationSkill<IPlayer>
    {
        private TeleportComponent _component;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Teleport.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/MageTeleport.dae");

        protected override float AnimationSpeed => .5f;

        protected override int MaxLevel => 20;
        public override float MaxCooldown => 54 - 20 * (Level / (float)MaxLevel);
        public override float ManaCost => 1;
        private float Distance => 128 + 128 * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("teleport_desc");
        public override string DisplayName => Translations.Get("teleport_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("teleport_distance_change", Distance.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void OnEnable()
        {
            base.OnEnable();
            User.AddComponent(_component = new TeleportComponent(User));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            User.RemoveComponent(_component);
            _component = null;
            var targetPosition = User.Position + User.View.LookingDirection * Distance;
            var hit = User.Physics.StaticRaycast(targetPosition, out var hitPosition);
            User.Position = hit ? hitPosition - User.View.LookingDirection * 2.5f : targetPosition;
        }

        private class TeleportComponent : Component<IHumanoid>
        {
            private static readonly VertexData ShieldMesh;
            private readonly ObjectMesh _meshObject;

            static TeleportComponent()
            {
                ShieldMesh = AssetManager.PLYLoader("Assets/Env/EnergyShield.ply", Vector3.One);
            }

            public TeleportComponent(IHumanoid Entity) : base(Entity)
            {
                _meshObject =
                    ObjectMesh.FromVertexData(ShieldMesh.Clone().Scale(Vector3.One * Parent.Model.Height * .8f));
                _meshObject.Alpha = .4f;
                _meshObject.Enabled = true;
                _meshObject.Outline = Parent.Model.Outline = true;
                _meshObject.OutlineColor = Parent.Model.OutlineColor = Colors.Violet * 2f;
                _meshObject.ApplySSAO = false;
                _meshObject.Scale = Vector3.Zero;
                Parent.Model.Outline = true;
                Parent.Model.OutlineColor = Vector4.Zero;
                SoundPlayer.PlaySound(SoundType.TeleportSound, Parent.Position);
            }

            public override void Update()
            {
                Parent.Model.OutlineColor =
                    Mathf.Lerp(Parent.Model.OutlineColor, Colors.Violet * 1.5f, Time.DeltaTime * 2f);
                _meshObject.Position = Parent.Model.ModelPosition + Vector3.UnitY * Parent.Model.Height * .5f;
                _meshObject.Rotation += Vector3.One * Time.DeltaTime * 2000f;
                _meshObject.Scale = Mathf.Lerp(_meshObject.Scale, Vector3.One, Time.DeltaTime * 2f);
            }

            public override void Dispose()
            {
                base.Dispose();
                TaskScheduler.While(() => (_meshObject.Scale - Vector3.Zero).LengthFast() > .005f,
                    () => { _meshObject.Scale = Mathf.Lerp(_meshObject.Scale, Vector3.Zero, Time.DeltaTime * 3f); });
                TaskScheduler.When(() => (_meshObject.Scale - Vector3.Zero).LengthFast() < .005f,
                    () => { _meshObject.Dispose(); });
                TaskScheduler.While(() => (Parent.Model.OutlineColor - Vector4.Zero).LengthFast() > .005f,
                    () =>
                    {
                        Parent.Model.OutlineColor =
                            Mathf.Lerp(Parent.Model.OutlineColor, Vector4.Zero, Time.DeltaTime * 2f);
                    });
                TaskScheduler.When(() => (Parent.Model.OutlineColor - Vector4.Zero).LengthFast() < .005f,
                    () => { Parent.Model.Outline = false; });
            }
        }
    }
}