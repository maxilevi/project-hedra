using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class GroundStomp : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/GroundStomp.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorGroundStomp.dae");

        protected override void OnAnimationMid()
        {
            VisualEffect();
            SkillUtils.DoNearby(User, Range, -1, (E, F) =>
            {
                E.Damage(Damage, User, out var xp);
                User.XP += xp;
                if(!E.IsDead && Utils.Rng.NextFloat() < StunChance)
                    E.KnockForSeconds(4f);
            });
        }

        private void VisualEffect()
        {
            var color = World.GetRegion(User.Position).Colors.DirtColor * .75f;
            World.Particles.VariateUniformly = true;
            World.Particles.GravityEffect = .25f;
            World.Particles.Scale = Vector3.One;
            World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
            World.Particles.PositionErrorMargin = new Vector3(4f, .5f, 4f);
            World.Particles.Shape = ParticleShape.Sphere;
            World.Particles.ParticleLifetime = 1.5f;

            for (var i = 0; i < 125; i++)
            {
                World.Particles.Position = User.Position + new Vector3(Utils.Rng.NextFloat() * 2f - 1f, 0, Utils.Rng.NextFloat() * 2f -1f) * Range;
                World.Particles.Direction = (Utils.Rng.NextFloat() * .5f + .5f) * Vector3.UnitY * 2f;
                World.Particles.Color = color;
                World.Particles.Emit();
            }
            World.HighlightArea(User.Position, color, Range, 1.5f);
        }

        protected override int MaxLevel => 15;
        public override float MaxCooldown => 22;
        private float StunChance => .15f + .35f * (Level / (float)MaxLevel);
        private float Damage => 10 + 30f * (Level / (float)MaxLevel);
        private float Range => 24 + 16f * (Level / (float)MaxLevel);
        public override float ManaCost => 0;
        public override string Description => Translations.Get("ground_stomp_desc");
        public override string DisplayName => Translations.Get("ground_stomp_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("ground_stomp_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ground_stomp_stun_change", (int) (StunChance * 100)),
            Translations.Get("ground_stomp_radius_change", Range.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}