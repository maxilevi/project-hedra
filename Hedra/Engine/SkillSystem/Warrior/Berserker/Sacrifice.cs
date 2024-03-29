using System.Globalization;
using System.Numerics;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Sacrifice : WeaponBonusWithAnimationSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Sacrifice.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/WarriorImbueAttack.dae");

        protected override float AnimationSpeed => 1.25f;

        protected override Vector4 OutlineColor => Colors.Red;
        protected override int MaxLevel => 15;
        private float BonusDamage => 30 + 70f * (Level / (float)MaxLevel);
        private float HealthPenalty => BonusDamage * .75f;
        protected override bool ShouldDisable => User.Health < HealthPenalty;
        public override float MaxCooldown => 17 - 3 * (Level / (float)MaxLevel);
        public override float ManaCost => 0;
        public override string Description => Translations.Get("sacrifice_desc");
        public override string DisplayName => Translations.Get("sacrifice_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("sacrifice_damage_bonus_change",
                BonusDamage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("sacrifice_penalty_change", HealthPenalty.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            Damage += BonusDamage;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            User.Damage(HealthPenalty, null, out _);
        }
    }
}