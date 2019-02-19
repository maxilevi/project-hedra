using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Concealment : SingleAnimationSkill
    {
        protected override int MaxLevel { get; }
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        protected override void OnExecution()
        {
            throw new System.NotImplementedException();
        }
        
        public override string Description => Translations.Get("concealment_desc");
        public override string DisplayName => Translations.Get("concealment_skill");
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Concealment.png");
    }
}