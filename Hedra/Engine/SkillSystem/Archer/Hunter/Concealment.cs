using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Concealment : SingleAnimationSkill
    {
        public override string Description { get; }
        public override string DisplayName { get; }
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Concealment.png");
        protected override int MaxLevel { get; }
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        protected override void OnExecution()
        {
            throw new System.NotImplementedException();
        }
    }
}