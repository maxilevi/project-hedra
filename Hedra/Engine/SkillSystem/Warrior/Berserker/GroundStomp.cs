using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class GroundStomp : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/GroundStomp.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("ground_stomp_desc");
        public override string DisplayName => Translations.Get("ground_stomp_skill");
    }
}