using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BoneSpirit : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BoneSpirit.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/NecromancerBoneSpirit.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("bone_spirit_desc");
        public override string DisplayName => Translations.Get("bone_spirit_skill");
    }
}