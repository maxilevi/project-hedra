using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Treason : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Treason.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("treason_desc");
        public override string DisplayName => Translations.Get("treason_skill");    
    }
}