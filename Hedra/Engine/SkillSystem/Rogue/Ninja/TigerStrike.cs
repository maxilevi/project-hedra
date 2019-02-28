using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class TigerStrike : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/TigerStrike.png");
        protected override Animation SkillAnimation { get; }  = AnimationLoader.LoadAnimation("Assets/Chr/RogueBladeRoundAttack.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("tiger_strike_desc");
        public override string DisplayName => Translations.Get("tiger_strike_skill");
    }
}