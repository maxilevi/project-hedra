using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Prayer : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Prayer.png");
        protected override Animation SkillAnimation { get; }
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("prayer_desc");
        public override string DisplayName => Translations.Get("prayer_skill");
    }
}