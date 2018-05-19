using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class HostileAIComponent : BaseAIComponent
    {
        public override AIType Type => AIType.Neutral;
        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }

        public HostileAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Roam.Update();
            }
        }
    }
}
