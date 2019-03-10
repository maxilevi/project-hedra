using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class RaiseSkeleton : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/RaiseSkeleton.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerRaiseSkeleton.dae");
        
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("raise_skeleton_desc");
        public override string DisplayName => Translations.Get("raise_skeleton_skill");
    }
}