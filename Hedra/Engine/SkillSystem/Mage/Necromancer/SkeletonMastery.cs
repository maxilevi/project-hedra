using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class SkeletonMastery : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SkeletonMastery.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 20;
        public override string Description  => Translations.Get("skeleton_mastery_desc");
        public override string DisplayName  => Translations.Get("skeleton_mastery_skill");
    }
}