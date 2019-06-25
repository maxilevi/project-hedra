using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class OakSpirit : MorphSkill
    {
        private AttackResistanceBonusComponent _attackResistance;
        private HealthBonusComponent _healthComponent;
               
        protected override void AddEffects()
        {
            User.AddComponent(_attackResistance = new AttackResistanceBonusComponent(User, User.AttackResistance * ResistanceBonus));
            User.AddComponent(_healthComponent = new HealthBonusComponent(User, HealthBonus));
            User.Health += HealthBonus;
        }

        protected override void RemoveEffects()
        {
            User.RemoveComponent(_attackResistance);
            User.RemoveComponent(_healthComponent);
            _attackResistance = null;
            _healthComponent = null;
        }

        protected override void ApplyVisuals(AnimatedModel Model, string ModelPath)
        {
            var region = World.BiomePool.GetRegion(User.Position);
            var woodColor = region.Colors.WoodColors[Utils.Rng.Next(0, region.Colors.WoodColors.Length)];
            AnimatedUpdatableModel.Paint(Model, ModelPath, new []
            {
                woodColor,
                region.Colors.LeavesColors[Utils.Rng.Next(0, region.Colors.LeavesColors.Length)],
                woodColor * .65f
            });
        }

        private float HealthBonus => 80 + 200 * (Level / (float) MaxLevel);
        private float ResistanceBonus => .3f + .7f * (Level / (float) MaxLevel);
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/OakSpirit.png");
        public override float ManaCost => 70;
        public override string Description => Translations.Get("oak_spirit_desc");
        public override string DisplayName => Translations.Get("oak_spirit_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("morph_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("oak_spirit_health_bonus", HealthBonus.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("oak_spirit_resistance_bonus", (int)(ResistanceBonus * 100))
        };
        protected override HumanType Type => HumanType.EntMorph;
        protected override bool RestrictWeapons => false;
        protected override bool CanUseOtherSkills => true;
    }
}