using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class Terror : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Terror.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerTerror.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("terror_desc");
        public override string DisplayName => Translations.Get("terror_skill");
    }
}