using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class FearAIComponent : BasicAIComponent
    {
        protected RunAwayBehaviour RunAway { get; }

        public FearAIComponent(IEntity Parent, IEntity Attacker) : base(Parent)
        {
            RunAway = new RunAwayBehaviour(Parent, Attacker);
        }

        public override void Update()
        {
            if (RunAway.Enabled)
            {
                RunAway.Update();
            }
        }

        public override AIType Type => AIType.Neutral;
    }
}