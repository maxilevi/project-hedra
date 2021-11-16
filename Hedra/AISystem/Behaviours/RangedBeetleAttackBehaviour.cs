using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class RangedBeetleAttackBehaviour : BaseBeetleAttackBehaviour
    {
        public RangedBeetleAttackBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override float SpitCooldown => 3.5f;
        protected override bool HasSpit => true;

        protected override Animation GetBiteAnimation(QuadrupedModel Model)
        {
            return Model.AttackAnimations[0];
        }

        protected override Animation GetSpitAnimation(QuadrupedModel Model)
        {
            return Model.AttackAnimations[1];
        }
    }
}