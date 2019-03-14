using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class CompanionMastery : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/CompanionMastery.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 20;
        public override string Description => Translations.Get("companion_mastery_desc");
        public override string DisplayName => Translations.Get("companion_mastery_skill");
    }
}