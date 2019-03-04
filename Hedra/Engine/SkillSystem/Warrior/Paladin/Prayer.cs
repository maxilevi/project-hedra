using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Prayer : Salvation
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Prayer.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorPrayer.dae");
        protected override Animation StanceAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorPrayerStance.dae");
        protected override bool EquipWeapons => false;

        protected override void OnHeal(IEntity Entity, float Dot)
        {
            base.OnHeal(Entity, Dot);
            if (!Entity.IsFriendly)
            {
                Entity.Damage(DamageBonus * (DamageInterval / Duration), Player, out var xp);
                Player.XP += xp;
            }
        }

        protected override float Duration => 6 + 7 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 64 - 10 * (Level / (float) MaxLevel);
        public override float ManaCost => 80;
        protected override float Radius => base.Radius * 1.5f;
        private float DamageBonus => 15 + 55f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("prayer_desc");
        public override string DisplayName => Translations.Get("prayer_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("prayer_area_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("prayer_damage_change", DamageBonus.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("prayer_heal_change", HealBonus.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}