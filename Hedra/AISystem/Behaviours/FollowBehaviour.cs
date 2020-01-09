using Hedra.Core;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class FollowBehaviour : Behaviour
    {
        public const int DefaultErrorMargin = 6;
        public IEntity Target { get; set; }
        protected TraverseBehaviour Traverse { get; }
        private readonly Timer _followTimer;

        public FollowBehaviour(IEntity Parent) : base(Parent)
        {
            _followTimer = new Timer(.5f);
            Traverse = new TraverseBehaviour(Parent);
            ErrorMargin = DefaultErrorMargin;
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
            if(_followTimer.Tick())
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