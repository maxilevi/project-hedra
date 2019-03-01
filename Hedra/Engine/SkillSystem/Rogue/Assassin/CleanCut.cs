using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class CleanCut : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/QuietSteps.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }
        
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("clean_cut_desc");
        public override string DisplayName => Translations.Get("clean_cut_skill");
    }
}