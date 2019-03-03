using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Stun : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Stun.png");
        protected override Animation SkillAnimation { get; }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("stun_desc");
        public override string DisplayName => Translations.Get("stun_skill");
    }
}