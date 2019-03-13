using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class Terror : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Terror.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerTerror.dae");
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => .75f;

        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
            World.HighlightArea(Player.Position, Vector4.One * .2f, Radius * 1.5f, .5f);
            SpawnParticles();
            SoundPlayer.PlaySound(SoundType.GroundQuake, Player.Position);
            SkillUtils.DoNearby(Player, Radius, E =>
            {
                E.AddComponent(new FearComponent(E, Player, Duration, Slowness));
            });
        }

        private void SpawnParticles()
        {
            World.Particles.Color = new Vector4(.2f, .2f, .2f, .8f);
            World.Particles.VariateUniformly = true;
            World.Particles.Position = Player.Position;
            World.Particles.GravityEffect = 0f;
            World.Particles.Scale = Vector3.One * .5f;
            World.Particles.ScaleErrorMargin = Vector3.One * .35f;
            World.Particles.PositionErrorMargin = Vector3.One * 2;
            World.Particles.Shape = ParticleShape.Sphere;       
            World.Particles.ParticleLifetime = .05f * Radius;       
            for(var i = 0; i < 500; i++)
            {
                var dir = new Vector3(Utils.Rng.NextFloat() * 2 - 1, 0f, Utils.Rng.NextFloat() * 2 - 1).NormalizedFast();
                World.Particles.Direction = dir;
                World.Particles.Emit();
            }
        }
        
        private float Radius => 24 + 32 * (Level / (float) MaxLevel);
        private float Duration => 4 + 5f * (Level / (float) MaxLevel);
        private float Slowness => 85 - 15f * (Level / (float) MaxLevel);
        public override float MaxCooldown => 32 - 8 * (Level / (float) MaxLevel) + Duration;
        public override float ManaCost => 45;
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("terror_desc");
        public override string DisplayName => Translations.Get("terror_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("terror_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("terror_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("terror_slowness_change", (int) (100-Slowness))
        };
    }
}