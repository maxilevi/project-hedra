using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    /// <inheritdoc />
    public class EscapeAIComponent : BasicAIComponent
    {
        public EscapeAIComponent(IEntity Parent, IEntity Target) : base(Parent)
        {
            Escape = new EscapeBehaviour(Parent)
            {
                Target = Target
            };
        }

        protected EscapeBehaviour Escape { get; set; }

        public override AIType Type => AIType.Neutral;

        public override void Update()
        {
            Escape.Update();
        }
    }
}