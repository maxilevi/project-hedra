using System;
using System.Numerics;
using Hedra.Engine;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class FleeBehaviour : Behaviour
    {
        public FleeBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        protected TraverseBehaviour Traverse { get; set; }
        public Func<Vector3> Target { get; protected set; }
        public float Radius { get; protected set; }

        public bool Enabled => Target != null;

        public void SetTarget(IEntity Target, float Radius)
        {
            this.Target = () => Target.Position;
            this.Radius = Radius;
        }

        public void SetTarget(Vector3 Point, float Radius)
        {
            Target = () => Point;
            this.Radius = Radius;
        }

        public override void Update()
        {
            if (Target != null)
            {
                var oppositeDirection = (Parent.Position - Target()).Xz().ToVector3().NormalizedFast();
                Traverse.SetTarget(Parent.Position + oppositeDirection * 16f);
                if ((Parent.Position - Target()).LengthSquared() > Radius * Radius) Target = null;
            }

            Traverse.Update();
        }

        public void Draw()
        {
        }

        public override void Dispose()
        {
            Traverse.Dispose();
            Target = null;
        }
    }
}