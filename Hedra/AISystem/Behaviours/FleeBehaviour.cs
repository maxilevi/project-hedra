using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class FleeBehaviour : Behaviour
    {
        protected TraverseBehaviour Traverse { get; set; }
        public Func<Vector3> Target { get; protected set; }
        public float Radius { get; protected set; }

        public FleeBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        public void SetTarget(IEntity Target, float Radius)
        {
            this.Target = () => Target.Position;
            this.Radius = Radius;
        }

        public void SetTarget(Vector3 Point, float Radius)
        {
            this.Target = () => Point;
            this.Radius = Radius;
        }

        public override void Update()
        {
            if (Target != null)
            {             
                var oppositeDirection = (Parent.Position - Target()).Xz().ToVector3().NormalizedFast();
                Traverse.SetTarget(Parent.Position + oppositeDirection * 16f);
                if ((Parent.Position - Target()).LengthSquared() > Radius * Radius)
                {
                    this.Target = null;
                }
            }
            Traverse.Update();
        }

        public bool Enabled => Target != null;
        
        public override void Dispose()
        {
            Traverse.Dispose();
            Target = null;
        }
    }
}
