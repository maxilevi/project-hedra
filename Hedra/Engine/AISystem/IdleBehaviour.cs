﻿using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class IdleBehaviour : Behaviour
    {
        public IdleBehaviour(Entity Parent) : base(Parent)
        {
        }

        public override void Update()
        {
            Parent.Model.Idle();
        }
    }
}
