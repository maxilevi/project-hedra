using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class FinishingBlow : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FinishingBlow.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("finishing_blow_desc");
        public override string DisplayName => Translations.Get("finishing_blow_skill");
    }
}