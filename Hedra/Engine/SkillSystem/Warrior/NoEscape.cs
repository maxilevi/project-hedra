using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class NoEscape : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/NoEscape.png");
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("no_escape_desc");
        public override string DisplayName => Translations.Get("no_escape_skill");
    }
}