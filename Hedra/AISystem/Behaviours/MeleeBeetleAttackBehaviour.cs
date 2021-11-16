using System;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class MeleeBeetleAttackBehaviour : BaseBeetleAttackBehaviour
    {
        public MeleeBeetleAttackBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override bool HasSpit => false;

        protected override Animation GetBiteAnimation(QuadrupedModel Model)
        {
            return Model.AttackAnimations.First();
        }

        protected override Animation GetSpitAnimation(QuadrupedModel Model)
        {
            throw new NotImplementedException();
        }
    }
}