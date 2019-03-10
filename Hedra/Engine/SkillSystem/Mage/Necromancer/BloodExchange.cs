using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BloodExchange : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BloodExchange.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerBloodExchange.dae");

        protected override void OnAnimationEnd()
        {
            base.OnAnimationEnd();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("blood_exchange_desc");
        public override string DisplayName => Translations.Get("blood_exchange_skill");
    }
}