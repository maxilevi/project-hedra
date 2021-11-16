using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class FearAIComponent : BasicAIComponent
    {
        public FearAIComponent(IEntity Parent, IEntity Attacker) : base(Parent)
        {
            RunAway = new RunAwayBehaviour(Parent, Attacker);
        }

        protected RunAwayBehaviour RunAway { get; }


        public override AIType Type => AIType.Neutral;

        public override void Update()
        {
            if (RunAway.Enabled) RunAway.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
            RunAway.Dispose();
        }
    }
}