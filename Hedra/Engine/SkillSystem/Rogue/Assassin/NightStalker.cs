using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class NightStalker : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/NightStalker.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }
        
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        public override string Description => Translations.Get("night_stalker_desc");
        public override string DisplayName => Translations.Get("night_stalker_skill");
        protected override int MaxLevel => 15;
    }
}