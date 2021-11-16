using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class GiantBeetleAttackBehaviour : BaseBeetleAttackBehaviour
    {
        private const int SpitAnimationIndex = 1;
        private const int BiteAnimationIndex = 0;

        public GiantBeetleAttackBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override float SpitCooldown => 5;
        protected override bool HasSpit => true;

        protected override Animation GetBiteAnimation(QuadrupedModel Model)
        {
            return Model.AttackAnimations[BiteAnimationIndex];
        }

        protected override Animation GetSpitAnimation(QuadrupedModel Model)
        {
            return Model.AttackAnimations[SpitAnimationIndex];
        }
    }
}