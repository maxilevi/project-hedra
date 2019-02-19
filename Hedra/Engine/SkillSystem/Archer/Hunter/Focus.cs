using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Focus : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Focus.png");
        
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        public override string Description => Translations.Get("focus_desc");
        public override string DisplayName => Translations.Get("focus_skill");
        protected override int MaxLevel { get; }
    }
}