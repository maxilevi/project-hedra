using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class RangedBeetleAIComponent : BaseBeetleAIComponent
    {
        public RangedBeetleAIComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override AttackBehaviour GetAttackBehaviour(IEntity Parent)
        {
            return new RangedBeetleAttackBehaviour(Parent);
        }
    }
}