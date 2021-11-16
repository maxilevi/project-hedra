using Hedra.Engine;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class EscapeBehaviour : Behaviour
    {
        public EscapeBehaviour(IEntity Parent) : base(Parent)
        {
            Traverse = new TraverseBehaviour(Parent);
        }

        public IEntity Target { get; set; }
        protected TraverseBehaviour Traverse { get; }

        public override void Update()
        {
            if (Parent.IsStuck || !Traverse.HasTarget &&
                (Target.Position - Parent.Position).Xz().ToVector3().LengthSquared() <
                GeneralSettings.UpdateDistanceSquared * .75f)
            {
                var targetDirection = (Target.Position - Parent.Position).Xz().ToVector3().NormalizedFast();
                Traverse.SetTarget(-targetDirection * 8f + Parent.Position);
            }

            Traverse.Update();
        }
    }
}