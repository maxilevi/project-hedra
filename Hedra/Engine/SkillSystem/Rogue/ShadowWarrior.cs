using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue
{
    public class ShadowWarrior : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ShadowWarrior.png");
        protected override void DoEnable()
        {
            throw new System.NotImplementedException();
        }

        protected override void DoDisable()
        {
            throw new System.NotImplementedException();
        }

        protected override float Duration => throw new System.NotImplementedException();
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("shadow_warrior_desc");
        public override string DisplayName => Translations.Get("shadow_warrior_skill");
    }
}