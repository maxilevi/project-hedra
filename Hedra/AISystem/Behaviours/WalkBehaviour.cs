using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class WalkBehaviour : Behaviour
    {
        public const float DefaultErrorMargin = 4;
        private bool _arrived;
        private Action _callback;

        public WalkBehaviour(IEntity Parent) : base(Parent)
        {
        }

        public Vector3 Target { get; private set; }
        public float ErrorMargin { get; set; } = DefaultErrorMargin;

        public bool HasTarget { get; private set; }

        public void Cancel()
        {
            _arrived = true;
            _callback?.Invoke();
            _callback = null;
            Target = Vector3.Zero;
            HasTarget = false;
        }

        public void SetTarget(Vector3 Point, Action Callback = null)
        {
            _arrived = false;
            _callback = Callback;
            Target = Point;
            HasTarget = true;
        }

        public override void Update()
        {
            if (!_arrived && !Parent.IsKnocked)
            {
                var orientation = (Target - Parent.Position).Xz().NormalizedFast().ToVector3();
                /* If the target reaches the point then (Target - Parent.Position) will be equal to Vector3.Zero and thus the normal will be Vector3.Zero too. */
                if (orientation != Vector3.Zero)
                {
                    var type = Parent.Type;
                    Parent.Orientation = Mathf.Lerp(Parent.Orientation, orientation, Time.DeltaTime * 8f);
                    Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                }
                else
                {
                    var a = 0;
                }

                Parent.Physics.Move();
                if ((Target - Parent.Position).Xz().LengthSquared() < ErrorMargin * ErrorMargin) Cancel();
            }
        }
    }
}