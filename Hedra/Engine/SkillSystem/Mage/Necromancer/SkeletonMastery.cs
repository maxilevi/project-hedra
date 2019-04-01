using System.Globalization;
using Hedra.API;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class SkeletonMastery : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SkeletonMastery.png");
        
        protected override void Add()
        {
            /* Do nothing because the stats are accessed directly by the RaiseSkeleton skill */
        }

        protected override void Remove()
        {
            /* Do nothing because the stats are accessed directly by the RaiseSkeleton skill */
        }

        protected override int MaxLevel => 20;
        public float HealthBonus => 0 + 140 * (Level / (float) MaxLevel);
        public int SkeletonLevel => 1 + (int) (5f * (Level / (float) MaxLevel));
        public float AttackResistance => .5f + .4f * (Level / (float) MaxLevel);
        public float AttackPower => .3f;
        private float ActualHealth => Humanoid.CalculateMaxHealth(ClassDesign.FromString(Class.Warrior), SkeletonLevel, 1) + HealthBonus;
        public override string Description  => Translations.Get("skeleton_mastery_desc");
        public override string DisplayName  => Translations.Get("skeleton_mastery_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("skeleton_mastery_resistance_change", (int) ((1f - AttackResistance) * 100)),
            Translations.Get("skeleton_mastery_health_change", ActualHealth.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("skeleton_mastery_level_change", SkeletonLevel)
        };
    }
}