using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class FollowBehaviour : Behaviour
    {
        public IEntity Target { get; set; }
        protected WalkBehaviour Walk { get; }

        public FollowBehaviour(IEntity Parent) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
        }

        public override void Update()
        {
            if (Enabled)
            {
                Walk.SetTarget(Target.Position);
            }
            else
            {
                Walk.Cancel();
            }
            Walk.Update();
        }

        public bool Enabled => Target != null && !Target.IsDead && !Target.IsInvisible;
    }
}
