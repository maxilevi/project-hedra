using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class CompanionMastery : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/CompanionMastery.png");
        
        protected override void Add(){}
        protected override void Remove(){}

        protected override int MaxLevel => 20;
        private float ConstantHealthReduction => .7f;
        private float ConstantResistanceReduction => .5f;
        private float ConstantDamageReduction => .4f;
        private float DynamicHealthMultiplier => .1f + .60f * (Level / (float) MaxLevel);
        private float DynamicResistanceMultiplier => .05f + .70f * (Level / (float) MaxLevel);
        private float DynamicDamageMultiplier => .1f + .4f * (Level / (float) MaxLevel);
        public float HealthMultiplier => ConstantHealthReduction + DynamicHealthMultiplier;
        public float ResistanceMultiplier => ConstantResistanceReduction + DynamicResistanceMultiplier;
        public float DamageMultiplier => ConstantDamageReduction + DynamicDamageMultiplier;
        public override string Description => Translations.Get("companion_mastery_desc");
        public override string DisplayName => Translations.Get("companion_mastery_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("companion_mastery_health_all_change", (int)(DynamicHealthMultiplier * 100)),
            Translations.Get("companion_mastery_resistance_all_change", (int)(DynamicResistanceMultiplier * 100)),
            Translations.Get("companion_mastery_damage_all_change", (int)(DynamicDamageMultiplier * 100))
        };
    }
}