using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class WindOfChange : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/WindOfChange.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageIdle.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("wind_of_change_desc");
        public override string DisplayName => Translations.Get("wind_of_change_skill");
    }
}