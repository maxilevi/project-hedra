using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Faith : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Faith.png");
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("faith_desc");
        public override string DisplayName => Translations.Get("faith_skill");
    }
}