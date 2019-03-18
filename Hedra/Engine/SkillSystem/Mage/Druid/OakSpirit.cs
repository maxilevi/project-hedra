using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class OakSpirit : MorphSkill
    {
        private AttackResistanceBonusComponent _attackResistance;
        private HealthBonusComponent _healthComponent;
               
        protected override void AddEffects()
        {
            Player.AddComponent(_attackResistance = new AttackResistanceBonusComponent(Player, Player.AttackResistance * ResistanceBonus));
            Player.AddComponent(_healthComponent = new HealthBonusComponent(Player, HealthBonus));
            Player.Health += HealthBonus;
        }

        protected override void RemoveEffects()
        {
            Player.RemoveComponent(_attackResistance);
            Player.RemoveComponent(_healthComponent);
            _attackResistance = null;
            _healthComponent = null;
        }

        protected override void ApplyVisuals(AnimatedModel Model, string ModelPath)
        {
            var region = World.BiomePool.GetRegion(Player.Position);
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
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/OakSpirit.png");
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
    }
}