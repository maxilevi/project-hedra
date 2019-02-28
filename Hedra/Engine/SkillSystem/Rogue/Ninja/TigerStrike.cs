using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class TigerStrike : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TigerStrike.png");
        protected override Animation SkillAnimation { get; }  = AnimationLoader.LoadAnimation("Assets/Chr/RogueTigerStrike.dae");
        protected override bool EquipWeapons => false;
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => 1.5f;

        protected override void OnAnimationMid()
        {
            SkillUtils.DoNearby(Player, 20f, .85f, (Entity, F) =>
            {
                Entity.Damage(Damage * F, Player, out var xp);
                Player.XP += xp;
                if (Utils.Rng.NextFloat() < StunChance)
                {
                    Player.KnockForSeconds(StunTime);
                }
            });
        }

        public override float ManaCost => 0;
        public override float MaxCooldown => 24;
        private float Damage => 25f + 40f *  (Level / (float)MaxLevel);
        private float StunChance => .1f + .25f * (Level / (float) MaxLevel);
        private float StunTime => 2.5f + 2f  * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("tiger_strike_desc");
        public override string DisplayName => Translations.Get("tiger_strike_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("tiger_strike_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("tiger_strike_stun_change", (int)(StunChance * 100)),
            Translations.Get("tiger_strike_stun_time_change", StunTime.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}