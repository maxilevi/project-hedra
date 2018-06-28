﻿using System;
using OpenTK;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    internal class FleeBehaviour : Behaviour
    {
        protected WalkBehaviour Walk { get; set; }
        public Func<Vector3> Target { get; protected set; }
        public float Radius { get; protected set; }

        public FleeBehaviour(Entity Parent) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
        }

        public void SetTarget(Entity Target, float Radius)
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
                var oppositeDirection = (Parent.Position - Target()).Xz.ToVector3().NormalizedFast();
                Walk.SetTarget(Parent.Position + oppositeDirection * 4f);
                if ((Parent.Position - Target()).LengthSquared > Radius * Radius)
                {
                    this.Target = null;
                }
                Walk.Update();
            }
        }

        public bool Enabled => Target != null;
    }
}
