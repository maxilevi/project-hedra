using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class ManaImbue : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ManaImbue.png");
        
        protected override void DoUse()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("mana_imbue_desc");
        public override string DisplayName => Translations.Get("mana_imbue_skill");
    }
}