using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class MeleeBeetleAIComponent : BaseBeetleAIComponent
    {
        public MeleeBeetleAIComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override AttackBehaviour GetAttackBehaviour(IEntity Parent)
        {
            return new MeleeBeetleAttackBehaviour(Parent);
        }
    }
}