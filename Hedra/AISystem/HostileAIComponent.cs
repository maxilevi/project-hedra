using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public class HostileAIComponent : BasicAIComponent
    {
        protected RoamBehaviour Roam { get; }
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public HostileAIComponent(Entity Parent) : base(Parent)
        {
            Roam = new RoamBehaviour(Parent)
            {
                AlertTime = 12f
            };
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Hostile.Update();
                if (!Hostile.Enabled)
                {
                    Roam.Update();
                }
            }
        }
        
        public override AIType Type => AIType.Hostile;
    }
}
