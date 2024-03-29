using System.Globalization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class TigerStrike : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TigerStrike.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/RogueTigerStrike.dae");

        protected override bool EquipWeapons => false;
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => 1.5f;

        public override float ManaCost => 0;
        public override float MaxCooldown => 24;
        private float Damage => 25f + 40f * (Level / (float)MaxLevel);
        private float StunChance => .2f + .3f * (Level / (float)MaxLevel);
        private float StunTime => 2.5f + 2f * (Level / (float)MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("tiger_strike_desc");
        public override string DisplayName => Translations.Get("tiger_strike_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("tiger_strike_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("tiger_strike_stun_change", (int)(StunChance * 100)),
            Translations.Get("tiger_strike_stun_time_change", StunTime.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void OnAnimationMid()
        {
            SkillUtils.DoNearby(User, 20f, .85f, (Entity, F) =>
            {
                Entity.Damage(Damage * F, User, out var xp);
                User.XP += xp;
                if (Utils.Rng.NextFloat() < StunChance) User.KnockForSeconds(StunTime);
            });
        }
    }
}