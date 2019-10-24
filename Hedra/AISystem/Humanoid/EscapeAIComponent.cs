using Hedra.AISystem.Behaviours;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using System.Numerics;

namespace Hedra.AISystem.Humanoid
{
    /// <inheritdoc />
    public class EscapeAIComponent : BasicAIComponent
    {
        protected EscapeBehaviour Escape { get; set; }

        public EscapeAIComponent(IEntity Parent, IEntity Target) : base(Parent)
        {
            Escape = new EscapeBehaviour(Parent)
            {
                Target = Target
            };
        }

        public override void Update()
        {
            Escape.Update();
        }

        public override AIType Type => AIType.Neutral;
    }
}