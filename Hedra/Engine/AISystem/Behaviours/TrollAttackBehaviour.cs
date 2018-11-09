using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class TrollAttackBehaviour : AttackBehaviour
    {
        protected ChargeAttackBehaviour Charge { get; }
        
        public TrollAttackBehaviour(IEntity Parent) : base(Parent)
        {
            
        }

        public override void Update()
        {
            Charge.Update();
        }
    }
}