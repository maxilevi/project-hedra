using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class WoodSpikes : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/WoodSpikes.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/MageIdle.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("wood_spikes_desc");
        public override string DisplayName => Translations.Get("wood_spikes_skill");
    }
}