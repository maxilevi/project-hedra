using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class GuardHostileBehaviour : HostileBehaviour
    {
        public GuardHostileBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        public Vector3 GuardPosition { get; set; }
        protected float Radius { get; } = 64;
        protected TraverseBehaviour Traverse { get; }

        public override void Update()
        {
            if (Traverse.HasTarget)
                Traverse.Update();
            else
                base.Update();
        }

        protected override IEntity GetTarget()
        {
            var target = base.GetTarget();
            if ((GuardPosition - Parent.Position).Xz().LengthSquared() > Radius * Radius)
                Traverse.SetTarget(GuardPosition);
            else
                return target;
            return null;
        }
    }
}