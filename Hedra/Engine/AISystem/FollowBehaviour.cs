using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class FollowBehaviour : Behaviour
    {
        public Entity Target { get; set; }
        protected WalkBehaviour Walk { get; }

        public FollowBehaviour(Entity Parent) : base(Parent)
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
