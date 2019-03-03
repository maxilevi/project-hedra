using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class IronSkin : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/IronSkin.png");
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("iron_skin_desc");
        public override string DisplayName => Translations.Get("iron_skin_skill");
    }
}