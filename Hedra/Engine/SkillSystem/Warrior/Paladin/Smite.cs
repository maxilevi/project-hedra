using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Smite : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Smite.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("smite_desc");
        public override string DisplayName => Translations.Get("smite_skill");
    }
}