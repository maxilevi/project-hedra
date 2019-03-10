using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class SiphonBlood : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SiphonBlood.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerSiphonBlood.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("siphon_blood_desc");
        public override string DisplayName => Translations.Get("siphon_blood_skill");
    }
}