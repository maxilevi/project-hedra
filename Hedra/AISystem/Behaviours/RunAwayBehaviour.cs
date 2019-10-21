using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class RunAwayBehaviour : FollowBehaviour
    {
        public RunAwayBehaviour(IEntity Parent, IEntity Attacker) : base(Parent)
        {
            Target = Attacker;
        }

        protected override void SetPosition()
        {
            Traverse.SetTarget(Parent.Position - (Target.Position - Parent.Position).NormalizedFast() * 32);
        }
    }
}