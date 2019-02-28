using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class FanOfKnives : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FanOfKnives.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueShurikenThrow.dae");

        protected override void OnAnimationMid()
        {
            base.OnAnimationMid();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("fan_of_knives_desc");
        public override string DisplayName => Translations.Get("fan_of_knives_skill");
    }
}