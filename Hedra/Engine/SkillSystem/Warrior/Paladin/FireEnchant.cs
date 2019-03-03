using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class FireEnchant : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Faith.png");
        protected override void DoUse()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("fire_enchant_desc");
        public override string DisplayName => Translations.Get("fire_enchant_skill");
    }
}