using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class FollowBehaviour : Behaviour
    {
        public IEntity Target { get; set; }
        protected TraverseBehaviour Traverse { get; }

        public FollowBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        public override void Update()
        {
            if (Enabled)
                SetPosition();
            else
                Traverse.Cancel();
            Traverse.Update();
        }

        public void Cancel()
        {
            Target = null;
            Traverse.CancelWalk();
        }

        protected virtual void SetPosition()
        {
            Traverse.SetTarget(Target.Position);
        }
        
        public bool Enabled => Target != null && !Target.IsDead && !Target.IsInvisible && !Target.Disposed;

        public float ErrorMargin
        {
            get => Traverse.ErrorMargin;
            set => Traverse.ErrorMargin = value;
        }
        
        public override void Dispose()
        {
            Traverse.Dispose();
        }
    }
}