using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Stealth : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Stealth.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("stealth_desc");
        public override string DisplayName => Translations.Get("stealth_skill"); 
    }
}