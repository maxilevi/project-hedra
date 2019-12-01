using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class EscapeBehaviour : Behaviour
    {
        public IEntity Target { get; set; }
        protected TraverseBehaviour Traverse { get; }
        public EscapeBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        public override void Update()
        {
            if (Parent.IsStuck || !Traverse.HasTarget && (Target.Position - Parent.Position).Xz().ToVector3().LengthSquared() < GeneralSettings.UpdateDistanceSquared * .75f)
            {
                var targetDirection = (Target.Position - Parent.Position).Xz().ToVector3().NormalizedFast();
                Traverse.SetTarget(-targetDirection * 16f + Parent.Position);
            }
            Traverse.Update();
        }
    }
}