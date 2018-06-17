using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class WalkBehaviour : Behaviour
    {
        public Vector3 Target { get; private set; }
        private bool _arrived;
        private Action _callback;
        private Vector3 _lastPosition;

        public WalkBehaviour(Entity Parent) : base(Parent)
        {
        }

        public void Cancel()
        {
            _arrived = true;
            this._callback?.Invoke();
            this._callback = null;
            this.Target = Vector3.Zero;
        }

        public void SetTarget(Vector3 Point)
        {
            this.SetTarget(Point, null);
        }

        public void SetTarget(Vector3 Point, Action Callback)
        {
            this._arrived = false;
            this._callback = Callback;
            this.Target = Point;
        }

        public override void Update()
        {
            if (!_arrived && !Parent.Knocked)
            {
                Parent.Model.Run();
                Parent.Orientation = (Target - Parent.Position).Xz.NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                Parent.Physics.Move(Parent.Orientation * 5f * Parent.Speed * (float) Time.deltaTime);

                if ((Target - Parent.Position).Xz.LengthSquared < 4 * 4 || _lastPosition.Xz  == Parent.Position.Xz)
                {
                    this.Cancel();
                }
                _lastPosition = Parent.Position;
            }
        }
    }
}
