using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class TrollAttackBehaviour : AttackBehaviour
    {
        protected ChargeBehaviour Charge { get; }
        
        public TrollAttackBehaviour(IEntity Parent) : base(Parent)
        {
            Charge = new ChargeBehaviour(Parent);
        }

        public override void Update()
        {
            if (!Charge.IsCharging)
            {
                base.Update();
            }
            Charge.Update();
        }

        public override void SetTarget(IEntity Target)
        {
            base.SetTarget(Target);
            Charge.SetTarget(Target);
        }
    }
}