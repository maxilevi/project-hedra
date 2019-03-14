using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class OakSpirit : MorphSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/OakSpirit.png");
        protected override void DoEnable()
        {
            throw new System.NotImplementedException();
        }

        protected override void DoDisable()
        {
            throw new System.NotImplementedException();
        }

        protected override float Duration => 5;
        protected override float CooldownDuration => 1;
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("oak_spirit_desc");
        public override string DisplayName => Translations.Get("oak_spirit_skill");
    }
}