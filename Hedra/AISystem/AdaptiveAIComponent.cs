using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.AISystem
{
    public class AdaptiveAIComponent : HostileAIComponent
    {
        private readonly AttackAnimationTemplate[] _templates;

        public AdaptiveAIComponent(Entity Parent) : base(Parent)
        {
            var asQuadruped = (QuadrupedModel)Parent.Model;
            _templates = asQuadruped.AttackTemplates;
            AlterBehaviour<AttackBehaviour>(new AdaptiveAttackBehaviour(Parent, _templates));
        }
    }
}