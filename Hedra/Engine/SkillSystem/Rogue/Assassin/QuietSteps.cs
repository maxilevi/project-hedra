using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class QuietSteps : PassiveSkill
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
        public override string Description => Translations.Get("quiet_steps_desc");
        public override string DisplayName => Translations.Get("quiet_steps_skill");     
    }
}