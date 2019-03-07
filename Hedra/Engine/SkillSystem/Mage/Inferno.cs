using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Inferno : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skill/Inferno.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageInferno.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("inferno_desc");
        public override string DisplayName => Translations.Get("inferno_skill");
    }
}