using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class GuardHostileBehaviour : HostileBehaviour
    {
        public Vector3 GuardPosition { get; set; }
        protected float Radius { get; } = 64;
        protected WalkBehaviour Walk { get; }
        
        public GuardHostileBehaviour(IEntity Parent) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
        }

        public override void Update()
        {
            if(Walk.HasTarget)
                Walk.Update();
            else
                base.Update();
        }

        protected override IEntity GetTarget()
        {
            var target = base.GetTarget();
            if ((GuardPosition - Parent.Position).Xz.LengthSquared > Radius * Radius)
                Walk.SetTarget(GuardPosition);
            else
                return target;
            return null;
        }
    }
}