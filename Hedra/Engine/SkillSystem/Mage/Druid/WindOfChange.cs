using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.Particles;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class WindOfChange : ActivateDurationSkillWithSingleAnimation
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/WindOfChange.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageStaffGroundHit.dae");
        protected override float AnimationSpeed => 1.5f;
        private readonly Timer _timer = new Timer(.25f);
        private readonly ParticleSystem _particles = new ParticleSystem();
        private bool _isActive;

        public override void Update()
        {
            base.Update();
            if (_isActive)
            {
                SpawnEffect();
                PushEnemies();
                if (_timer.Tick())
                {
                    SkillUtils.DoNearby(User, PushDistance, 0, (E, F) =>
                    {
                        E.Damage(Damage * F * _timer.AlertTime / Duration, User, out var xp);
                        User.XP += xp;
                    });
                }
            }
        }

        protected override void DoEnable()
        {
            _isActive = true;
        }

        protected override void DoDisable()
        {
            _isActive = false;
        }

        private void SpawnEffect()
        {
            User.Movement.Orientate();
            _particles.Color = Vector4.One * 2f;
            _particles.VariateUniformly = true;
            _particles.Position = User.Position + Vector3.UnitY * User.Model.Height * .5f - User.LookingDirection.Xz.ToVector3() * 6f;
            _particles.GravityEffect = 0.0f;
            _particles.Scale = Vector3.One * .75f;
            _particles.ScaleErrorMargin = Vector3.One * .35f;
            _particles.PositionErrorMargin = new Vector3(16, 16, 16) * 3f;
            _particles.Shape = ParticleShape.Sphere;
            _particles.ParticleLifetime = Duration;
            _particles.Direction = User.LookingDirection * 5f;
            for(var i = 0; i < 15; ++i)
            {
                _particles.Emit();
            }
        }

        private void PushEnemies()
        {
            SkillUtils.DoNearby(User, PushDistance, 0, (E, F) =>
            {
                E.Physics.DeltaTranslate(-(User.Position - E.Position).Xz.NormalizedFast().ToVector3() * 80f);
            });
        }

        protected override int MaxLevel => 15;
        protected override float Duration => .75f;
        protected override float CooldownDuration => 24 - 6 * (Level / (float) MaxLevel);
        public override float ManaCost => 55;
        private float PushDistance => 48 + 48 * (Level / (float) MaxLevel);
        private float Damage => 15 + 20f * (Level / (float )MaxLevel);
        public override string Description => Translations.Get("wind_of_change_desc");
        public override string DisplayName => Translations.Get("wind_of_change_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("wind_of_change_distance_change", PushDistance.ToString("0.0")),
            Translations.Get("wind_of_change_damage_change", Damage.ToString("0.0"))
        };
    }
}