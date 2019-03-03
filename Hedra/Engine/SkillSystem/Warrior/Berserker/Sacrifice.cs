using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Sacrifice : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Sacrifice.png");
        protected override void DoUse()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("sacrifice_desc");
        public override string DisplayName => Translations.Get("sacrifice_skill");
    }
}