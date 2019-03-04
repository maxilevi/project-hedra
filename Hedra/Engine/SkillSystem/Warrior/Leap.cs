using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior
{
    public class Leap : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Leap.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("leap_desc");
        public override string DisplayName => Translations.Get("leap_skill");
    }
}