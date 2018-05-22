﻿using System;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class FleeBehaviour : Behaviour
    {
        protected WalkBehaviour Walk { get; set; }
        public Entity Target { get; protected set; }
        public float Radius { get; protected set; }

        public FleeBehaviour(Entity Parent) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
        }

        public void SetTarget(Entity Target, float Radius)
        {
            this.Target = Target;
            this.Radius = Radius;
        }

        public override void Update()
        {
            if (Target != null)
            {
                var oppositeDirection = (Parent.Position - Target.Position).Xz.ToVector3().NormalizedFast();
                Walk.SetTarget(Parent.Position + oppositeDirection * 4f);
                if ((Parent.Position - Target.Position).LengthSquared > Radius * Radius)
                {
                    this.Target = null;
                }
                Walk.Update();
            }
        }

        public bool Enabled => Target != null;
    }
}
