using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class WalkBehaviour : Behaviour
    {
        public Vector3 Target { get; private set; }
        private bool _arrived;
        private Action _callback;
        private Vector3 _lastPosition;
        
        public WalkBehaviour(IEntity Parent) : base(Parent)
        {
        }

        public void Cancel()
        {
            this._arrived = true;
            this._callback?.Invoke();
            this._callback = null;
            this.Target = Vector3.Zero;
            this.HasTarget = false;
        }

        public void SetTarget(Vector3 Point, Action Callback = null)
        {
            this._arrived = false;
            this._callback = Callback;
            this.Target = Point;
            this.HasTarget = true;
        }

        public override void Update()
        {
            if (!_arrived && !Parent.IsKnocked)
            {
                var orientation = (Target - Parent.Position).Xz.NormalizedFast().ToVector3();
                /* If the target reaches the point then (Target - Parent.Position) will be equal to Vector3.Zero and thus the normal will be Vector3.Zero too. */
                if (orientation != Vector3.Zero)
                {
                    Parent.Orientation = Mathf.Lerp(Parent.Orientation, orientation, Time.DeltaTime * 8f);
                    Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                }
                Parent.Physics.Move();
                if ((Target - Parent.Position).Xz.LengthSquared < 2 * 2)
                {
                    this.Cancel();
                }
            }
            Parent.IsStuck = !Parent.IsMoving && !_arrived && HasTarget || _lastPosition.Xz == Parent.Position.Xz && HasTarget;
            _lastPosition = Parent.Position;
        }
        
        public bool HasTarget { get; private set; }
    }
}
